Option Strict On
Option Explicit On

Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json
Imports System.Runtime.InteropServices
Imports System.Threading

Public Class frmMain
    Private foSorter As ListviewSorter

    <DllImport("user32.dll")>
    Private Shared Function SetForegroundWindow(ByVal poHwnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ShowWindowAsync(ByVal poHwnd As IntPtr, ByVal piCmdShow As Integer) As Boolean
    End Function

    Private Const miSW_RESTORE As Integer = 9


    ' --- icons ---
    Private Const ksIconFolderClosed As String = "folder_closed"
    Private Const ksIconFolderOpen As String = "folder_open"
    Private Const ksIconTextFile As String = "c_file"

    ' --- state ---
    Private fsUnityRoot As String = String.Empty
    Private fsAssetsRoot As String = String.Empty
    Private fbTreeBusy As Boolean = False

    ' --- search state ---
    Private fsLastSearchText As String = String.Empty
    Private foLastFoundNode As TreeNode = Nothing

    '---- excluded folders list
    Private Const ksExcludedFoldersSettingName As String = "ExcludedAssetFoldersJson"
    Private Const ksExcludedFoldersFallbackFileName As String = "ExcludedAssetFolders.json"
    Private foExcludedFolders As New List(Of String)

    '---- included files options
    Private fbIncludeMetaFiles As Boolean = False

    ' Broad set of Unity/project text files that are useful to include in a code pack.
    ' These are intentionally text-oriented file types only.
    Private Shared ReadOnly moAllowedExtensions As HashSet(Of String) =
        New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            ".cs",
            ".prefab",
            ".asset",
            ".unity",
            ".json",
            ".asmdef",
            ".uxml",
            ".uss",
            ".shader",
            ".compute",
            ".cginc",
            ".hlsl",
            ".mat",
            ".anim",
            ".controller",
            ".overridecontroller",
            ".inputactions",
            ".spriteatlasv2",
            ".txt",
            ".sql",
            ".md",
            ".xml",
            ".yml",
            ".yaml"
        }

    ' ---------- Form ----------
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim psUnity As String = My.Settings.UnityFolder
        Dim psExport As String = My.Settings.ExportFolder

        foExcludedFolders = LoadExcludedFoldersFromSettings()

        If Not String.IsNullOrWhiteSpace(psUnity) AndAlso Directory.Exists(psUnity) Then

            fsUnityRoot = psUnity.Trim()
            fsAssetsRoot = Path.Combine(fsUnityRoot, "Assets")

            If Directory.Exists(fsAssetsRoot) Then
                lblUnityFolder.Text = "Unity Folder: " & fsUnityRoot
                lblUnityFolder.ToolTipText = fsAssetsRoot
                BuildTree()
            Else
                fsUnityRoot = String.Empty
                fsAssetsRoot = String.Empty
                lblUnityFolder.Text = "Unity Folder:"
                lblUnityFolder.ToolTipText = String.Empty
                ClearAssetsTreeAndSelectedList()
            End If

        Else
            ClearAssetsTreeAndSelectedList()
        End If

        If Not String.IsNullOrWhiteSpace(psExport) AndAlso Directory.Exists(psExport) Then
            txtExportTo.Text = Path.Combine(psExport, "codepack.zip")
        End If

        cboOutput.Items.Clear()
        cboOutput.Items.Add("ZIP only")
        cboOutput.Items.Add("JSON and ZIP")
        cboOutput.SelectedIndex = 0

        ConfigureSelectedListView()
        SyncSelectedListViewFromTree()

        fbIncludeMetaFiles = btnIncludeMetaFiles.Checked

    End Sub

    Private Sub ConfigureSelectedListView()

        With lvwSelected
            .View = View.Details
            .MultiSelect = False
            .FullRowSelect = True
            .GridLines = True
            .Items.Clear()
            .SmallImageList = Nothing
            .Columns.Clear()
            .Columns.Add("FILENAME", "Selected Files", Math.Max(120, lvwSelected.ClientSize.Width - 8))
            foSorter = New ListviewSorter(0, SortOrder.Ascending, True)
            .ListViewItemSorter = foSorter
        End With

    End Sub

    Private Sub frmMain_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        ResizeSelectedListViewColumn()
    End Sub

    Private Sub lvwSelected_Resize(sender As Object, e As EventArgs) Handles lvwSelected.Resize
        ResizeSelectedListViewColumn()
    End Sub

    Private Sub ResizeSelectedListViewColumn()

        If lvwSelected.Columns.Count = 0 Then Exit Sub

        lvwSelected.Columns(0).Width = Math.Max(120, lvwSelected.ClientSize.Width - 8)

    End Sub

    ' ------------------- exclude folders selection 
    Private Sub btnExclude_Click(sender As Object, e As EventArgs) Handles btnExclude.Click

        If String.IsNullOrWhiteSpace(fsAssetsRoot) OrElse Not Directory.Exists(fsAssetsRoot) Then
            MessageBox.Show(Me, "Select a valid Unity project folder first.", "Missing Assets Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim poCheckedFiles As New HashSet(Of String)(GetCheckedFiles(), StringComparer.OrdinalIgnoreCase)
        Dim poExpandedFolders As HashSet(Of String) = GetExpandedFolderPaths()
        Dim psSelectedPath As String = GetSelectedNodePath()

        Using poFrm As New frmExcludeFolders
            poFrm.Initialize(fsAssetsRoot, foExcludedFolders)

            If poFrm.ShowDialog(Me) = DialogResult.OK Then
                foExcludedFolders = NormalizeExcludedFolderList(poFrm.SelectedFolderList)
                SaveExcludedFoldersToSettings()
                BuildTree(poCheckedFiles, poExpandedFolders, psSelectedPath)
            End If
        End Using

    End Sub



    Private Function LoadExcludedFoldersFromSettings() As List(Of String)

        Dim poList As New List(Of String)()

        Try
            Dim psJson As String = GetStringSettingValue(ksExcludedFoldersSettingName)

            If String.IsNullOrWhiteSpace(psJson) Then
                psJson = ReadExcludedFoldersFallbackFile()
            End If

            If Not String.IsNullOrWhiteSpace(psJson) Then
                Dim poLoaded As List(Of String) = JsonConvert.DeserializeObject(Of List(Of String))(psJson)

                If poLoaded IsNot Nothing Then
                    poList = poLoaded
                End If
            End If

        Catch
            poList = New List(Of String)()
        End Try

        Return NormalizeExcludedFolderList(poList)

    End Function

    Private Sub SaveExcludedFoldersToSettings()

        Dim psJson As String = JsonConvert.SerializeObject(NormalizeExcludedFolderList(foExcludedFolders), Formatting.None)

        If SetStringSettingValue(ksExcludedFoldersSettingName, psJson) Then
            My.Settings.Save()
            Return
        End If

        WriteExcludedFoldersFallbackFile(psJson)

    End Sub

    Private Function GetStringSettingValue(ByVal sSettingName As String) As String

        Try
            If My.Settings.Properties(sSettingName) Is Nothing Then Return String.Empty

            Dim poValue As Object = My.Settings(sSettingName)
            If poValue Is Nothing Then Return String.Empty

            Return Convert.ToString(poValue)

        Catch
            Return String.Empty
        End Try

    End Function

    Private Function SetStringSettingValue(ByVal sSettingName As String, ByVal sValue As String) As Boolean

        Try
            If My.Settings.Properties(sSettingName) Is Nothing Then Return False

            My.Settings(sSettingName) = sValue
            Return True

        Catch
            Return False
        End Try

    End Function

    Private Function ReadExcludedFoldersFallbackFile() As String

        Try
            Dim psPath As String = GetExcludedFoldersFallbackPath()

            If Not File.Exists(psPath) Then Return String.Empty

            Return File.ReadAllText(psPath, Encoding.UTF8)

        Catch
            Return String.Empty
        End Try

    End Function

    Private Sub WriteExcludedFoldersFallbackFile(ByVal sJson As String)

        Dim psPath As String = GetExcludedFoldersFallbackPath()
        Dim psFolder As String = Path.GetDirectoryName(psPath)

        If Not String.IsNullOrWhiteSpace(psFolder) Then
            Directory.CreateDirectory(psFolder)
        End If

        File.WriteAllText(psPath, sJson, New UTF8Encoding(False))

    End Sub

    Private Function GetExcludedFoldersFallbackPath() As String

        Dim psCompanyName As String = Application.CompanyName
        Dim psProductName As String = Application.ProductName

        If String.IsNullOrWhiteSpace(psCompanyName) Then psCompanyName = "CodePacker"
        If String.IsNullOrWhiteSpace(psProductName) Then psProductName = "CodePacker"

        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), psCompanyName, psProductName, ksExcludedFoldersFallbackFileName)

    End Function

    Private Function NormalizeExcludedFolderList(ByVal oFolderList As IEnumerable(Of String)) As List(Of String)

        Dim poOut As New List(Of String)()
        Dim poSeen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        If oFolderList Is Nothing Then Return poOut

        For Each psFolder As String In oFolderList

            Dim psRelativeFolder As String = NormalizeAssetRelativeFolder(psFolder)

            If String.IsNullOrWhiteSpace(psRelativeFolder) Then Continue For
            If poSeen.Contains(psRelativeFolder) Then Continue For

            poSeen.Add(psRelativeFolder)
            poOut.Add(psRelativeFolder)

        Next

        poOut.Sort(StringComparer.OrdinalIgnoreCase)
        Return poOut

    End Function

    Private Function NormalizeAssetRelativeFolder(ByVal sFolder As String) As String

        If String.IsNullOrWhiteSpace(sFolder) Then Return String.Empty

        Dim psFolder As String = sFolder.Trim().Replace("\", "/")

        If Path.IsPathRooted(psFolder) Then
            psFolder = GetAssetRelativeFolderPath(psFolder)
        End If

        If psFolder.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) Then
            psFolder = psFolder.Substring("Assets/".Length)
        ElseIf String.Equals(psFolder, "Assets", StringComparison.OrdinalIgnoreCase) Then
            psFolder = String.Empty
        End If

        psFolder = psFolder.Trim("/"c)

        While psFolder.Contains("//")
            psFolder = psFolder.Replace("//", "/")
        End While

        Return psFolder

    End Function

    Private Function GetAssetRelativeFolderPath(ByVal sFolder As String) As String

        If String.IsNullOrWhiteSpace(sFolder) Then Return String.Empty
        If String.IsNullOrWhiteSpace(fsAssetsRoot) Then Return String.Empty

        Try
            Dim psFull As String = Path.GetFullPath(sFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            Dim psAssets As String = Path.GetFullPath(fsAssetsRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            Dim psAssetsPrefix As String = psAssets & Path.DirectorySeparatorChar

            If String.Equals(psFull, psAssets, StringComparison.OrdinalIgnoreCase) Then Return String.Empty

            If psFull.StartsWith(psAssetsPrefix, StringComparison.OrdinalIgnoreCase) Then
                Return psFull.Substring(psAssetsPrefix.Length).Replace("\", "/").Trim("/"c)
            End If

        Catch
            Return String.Empty
        End Try

        Return String.Empty

    End Function

    Private Sub ClearAssetsTreeAndSelectedList()

        tvwFiles.Nodes.Clear()
        lvwSelected.Items.Clear()
        ResetSearchState()

    End Sub

    ' ---------- Unity folder selection ----------
    Private Sub btnSelectUnity_Click(sender As Object, e As EventArgs) Handles btnSelectUnity.Click

        Dim poDlg As New FolderBrowserDialog()
        poDlg.Description = "Select Unity project root folder (the folder that contains Assets\)"
        poDlg.UseDescriptionForTitle = True

        If Directory.Exists(fsUnityRoot) Then poDlg.SelectedPath = fsUnityRoot
        If poDlg.ShowDialog(Me) <> DialogResult.OK Then Exit Sub

        fsUnityRoot = poDlg.SelectedPath.Trim()
        fsAssetsRoot = Path.Combine(fsUnityRoot, "Assets")

        If Not Directory.Exists(fsAssetsRoot) Then
            MessageBox.Show(Me, "That folder does not contain an Assets\ folder. Pick the Unity project root.", "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            fsUnityRoot = String.Empty
            fsAssetsRoot = String.Empty
            lblUnityFolder.Text = "Unity Folder:"
            lblUnityFolder.ToolTipText = String.Empty
            ClearAssetsTreeAndSelectedList()
            Exit Sub
        End If

        lblUnityFolder.Text = "Unity Folder: " & fsUnityRoot
        lblUnityFolder.ToolTipText = fsAssetsRoot

        My.Settings.UnityFolder = fsUnityRoot
        My.Settings.Save()

        BuildTree()

    End Sub

    Private Sub btnIncludeMetaFiles_Click(sender As Object, e As EventArgs) Handles btnIncludeMetaFiles.Click
        fbIncludeMetaFiles = btnIncludeMetaFiles.Checked
    End Sub

    Private Sub BuildTree(Optional oCheckedFiles As HashSet(Of String) = Nothing, Optional oExpandedFolders As HashSet(Of String) = Nothing, Optional sSelectedPath As String = "")

        ResetSearchState()

        If String.IsNullOrWhiteSpace(fsAssetsRoot) OrElse Not Directory.Exists(fsAssetsRoot) Then
            ClearAssetsTreeAndSelectedList()
            Exit Sub
        End If

        fbTreeBusy = True
        tvwFiles.BeginUpdate()

        Try

            tvwFiles.Nodes.Clear()

            Dim poRootNode As TreeNode = CreateFolderNode(fsAssetsRoot)
            tvwFiles.Nodes.Add(poRootNode)

            If oCheckedFiles IsNot Nothing AndAlso oCheckedFiles.Count > 0 Then
                ApplyCheckedFilesToTree(poRootNode, oCheckedFiles)
                UpdateFolderChecksRecursive(poRootNode)
            End If

            If oExpandedFolders IsNot Nothing AndAlso oExpandedFolders.Count > 0 Then
                ApplyExpandedFoldersToTree(poRootNode, oExpandedFolders)
            Else
                poRootNode.Expand()
            End If

            If Not String.IsNullOrWhiteSpace(sSelectedPath) Then
                Dim poSelectedNode As TreeNode = FindNodeByPath(sSelectedPath)

                If poSelectedNode IsNot Nothing Then
                    ExpandAncestors(poSelectedNode)
                    tvwFiles.SelectedNode = poSelectedNode
                    poSelectedNode.EnsureVisible()
                End If
            End If

        Finally
            tvwFiles.EndUpdate()
            fbTreeBusy = False
        End Try

        SyncSelectedListViewFromTree()

    End Sub

    Private Function CreateFolderNode(sFolder As String) As TreeNode

        Dim psName As String = Path.GetFileName(sFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        If String.IsNullOrWhiteSpace(psName) Then psName = sFolder

        Dim poNode As New TreeNode(psName)
        poNode.Tag = sFolder
        poNode.ImageKey = ksIconFolderClosed
        poNode.SelectedImageKey = ksIconFolderOpen

        Dim paDirs() As String = Array.Empty(Of String)()

        Try
            paDirs = Directory.GetDirectories(sFolder)
        Catch
            paDirs = Array.Empty(Of String)()
        End Try

        Array.Sort(paDirs, StringComparer.OrdinalIgnoreCase)

        For Each psDir As String In paDirs
            If ShouldSkipDirectory(psDir) Then Continue For
            poNode.Nodes.Add(CreateFolderNode(psDir))
        Next

        Dim paFiles() As String = Array.Empty(Of String)()

        Try
            paFiles = Directory.GetFiles(sFolder, "*.*", SearchOption.TopDirectoryOnly)
        Catch
            paFiles = Array.Empty(Of String)()
        End Try

        Array.Sort(paFiles, StringComparer.OrdinalIgnoreCase)

        For Each psFile As String In paFiles
            If Not IsAllowedFile(psFile) Then Continue For

            Dim poFileNode As New TreeNode(Path.GetFileName(psFile))
            poFileNode.Tag = psFile
            poFileNode.ImageKey = ksIconTextFile
            poFileNode.SelectedImageKey = ksIconTextFile
            poNode.Nodes.Add(poFileNode)
        Next

        Return poNode

    End Function

    Private Function ShouldSkipDirectory(sFolder As String) As Boolean

        Dim psName As String = Path.GetFileName(sFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))

        If String.IsNullOrWhiteSpace(psName) Then Return False

        Select Case psName.ToLowerInvariant()
            Case "library", "temp", "obj", ".git", "builds", "packagesettings", "usersettings"
                Return True
        End Select

        If IsExcludedAssetFolder(sFolder) Then Return True

        Return False

    End Function

    Private Function IsExcludedAssetFolder(ByVal sFolder As String) As Boolean

        Dim psRelativeFolder As String = GetAssetRelativeFolderPath(sFolder)

        If String.IsNullOrWhiteSpace(psRelativeFolder) Then Return False

        For Each psExcludedFolder As String In foExcludedFolders

            Dim psCleanExcludedFolder As String = NormalizeAssetRelativeFolder(psExcludedFolder)

            If String.IsNullOrWhiteSpace(psCleanExcludedFolder) Then Continue For

            If String.Equals(psRelativeFolder, psCleanExcludedFolder, StringComparison.OrdinalIgnoreCase) Then Return True

            If psRelativeFolder.StartsWith(psCleanExcludedFolder & "/", StringComparison.OrdinalIgnoreCase) Then Return True

        Next

        Return False

    End Function

    Private Function IsAllowedFile(sFilePath As String) As Boolean

        Dim psExt As String = Path.GetExtension(sFilePath)
        If String.IsNullOrWhiteSpace(psExt) Then Return False

        Return moAllowedExtensions.Contains(psExt)

    End Function

    ' ---------- Select all / none ----------
    Private Sub btnSelectAll_Click(sender As Object, e As EventArgs) Handles btnSelectAll.Click
        SetAllChecks(True)
    End Sub

    Private Sub btnSelectNone_Click(sender As Object, e As EventArgs) Handles btnSelectNone.Click
        SetAllChecks(False)
    End Sub

    Private Sub SetAllChecks(bChecked As Boolean)

        fbTreeBusy = True

        If tvwFiles.Nodes.Count > 0 Then
            tvwFiles.BeginUpdate()

            For Each poNode As TreeNode In tvwFiles.Nodes
                SetNodeCheckedRecursive(poNode, bChecked)
            Next

            tvwFiles.EndUpdate()
        End If

        fbTreeBusy = False
        SyncSelectedListViewFromTree()

    End Sub

    Private Sub SetNodeCheckedRecursive(oNode As TreeNode, bChecked As Boolean)

        oNode.Checked = bChecked

        For Each poChild As TreeNode In oNode.Nodes
            SetNodeCheckedRecursive(poChild, bChecked)
        Next

    End Sub

    ' ---------- TreeView checkbox behavior ----------
    Private Sub tvwFiles_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles tvwFiles.AfterCheck

        If fbTreeBusy Then Exit Sub
        fbTreeBusy = True

        For Each poChild As TreeNode In e.Node.Nodes
            SetNodeCheckedRecursive(poChild, e.Node.Checked)
        Next

        UpdateParentsFromChildren(e.Node)

        fbTreeBusy = False
        SyncSelectedListViewFromTree()

    End Sub

    Private Sub tvwFiles_AfterExpand(sender As Object, e As TreeViewEventArgs) Handles tvwFiles.AfterExpand
        If IsFolderNode(e.Node) Then e.Node.ImageKey = ksIconFolderOpen
    End Sub

    Private Sub tvwFiles_AfterCollapse(sender As Object, e As TreeViewEventArgs) Handles tvwFiles.AfterCollapse
        If IsFolderNode(e.Node) Then e.Node.ImageKey = ksIconFolderClosed
    End Sub

    Private Sub tvwFiles_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvwFiles.NodeMouseClick

        If e.Node Is Nothing Then Exit Sub

        If e.Node.Nodes.Count > 0 Then
            If e.Node.IsExpanded Then
                e.Node.Collapse()
            Else
                e.Node.Expand()
            End If
        End If

    End Sub

    Private Function IsFolderNode(oNode As TreeNode) As Boolean

        Dim psTag As String = TryCast(oNode.Tag, String)
        If String.IsNullOrWhiteSpace(psTag) Then Return False

        Return Directory.Exists(psTag)

    End Function

    Private Sub UpdateParentsFromChildren(oNode As TreeNode)

        Dim poParent As TreeNode = oNode.Parent

        While poParent IsNot Nothing

            Dim pbAnyChecked As Boolean = False

            For Each poChild As TreeNode In poParent.Nodes
                If poChild.Checked Then
                    pbAnyChecked = True
                    Exit For
                End If
            Next

            poParent.Checked = pbAnyChecked
            poParent = poParent.Parent

        End While

    End Sub

    Private Sub SyncSelectedListViewFromTree()

        Dim poCheckedFiles As List(Of String) = GetCheckedFiles()

        poCheckedFiles.Sort(StringComparer.OrdinalIgnoreCase)

        lvwSelected.BeginUpdate()

        Try
            lvwSelected.Items.Clear()

            For Each psFile As String In poCheckedFiles
                Dim poItem As New ListViewItem(Path.GetFileName(psFile))
                poItem.Name = psFile
                poItem.Tag = psFile
                poItem.ToolTipText = psFile
                lvwSelected.Items.Add(poItem)
            Next

            ResizeSelectedListViewColumn()

        Finally
            lvwSelected.Sort()
            lvwSelected.EndUpdate()
        End Try

    End Sub

    Private Sub ApplyCheckedFilesToTree(oNode As TreeNode, oCheckedFiles As HashSet(Of String))

        If oNode Is Nothing OrElse oCheckedFiles Is Nothing Then Exit Sub

        Dim psTag As String = TryCast(oNode.Tag, String)

        If Not String.IsNullOrWhiteSpace(psTag) AndAlso File.Exists(psTag) AndAlso IsAllowedFile(psTag) Then
            oNode.Checked = oCheckedFiles.Contains(psTag)
        End If

        For Each poChild As TreeNode In oNode.Nodes
            ApplyCheckedFilesToTree(poChild, oCheckedFiles)
        Next

    End Sub

    Private Function UpdateFolderChecksRecursive(oNode As TreeNode) As Boolean

        If oNode Is Nothing Then Return False

        If Not IsFolderNode(oNode) Then
            Return oNode.Checked
        End If

        Dim pbAnyChildChecked As Boolean = False

        For Each poChild As TreeNode In oNode.Nodes
            If UpdateFolderChecksRecursive(poChild) Then
                pbAnyChildChecked = True
            End If
        Next

        oNode.Checked = pbAnyChildChecked
        Return pbAnyChildChecked

    End Function

    Private Sub ApplyExpandedFoldersToTree(oNode As TreeNode, oExpandedFolders As HashSet(Of String))

        If oNode Is Nothing OrElse oExpandedFolders Is Nothing Then Exit Sub

        Dim psTag As String = TryCast(oNode.Tag, String)

        If Not String.IsNullOrWhiteSpace(psTag) AndAlso Directory.Exists(psTag) AndAlso oExpandedFolders.Contains(psTag) Then
            oNode.Expand()
        End If

        For Each poChild As TreeNode In oNode.Nodes
            ApplyExpandedFoldersToTree(poChild, oExpandedFolders)
        Next

    End Sub

    Private Function GetExpandedFolderPaths() As HashSet(Of String)

        Dim poExpandedFolders As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each poNode As TreeNode In tvwFiles.Nodes
            CollectExpandedFolderPathsRecursive(poNode, poExpandedFolders)
        Next

        Return poExpandedFolders

    End Function

    Private Sub CollectExpandedFolderPathsRecursive(oNode As TreeNode, oOut As HashSet(Of String))

        If oNode Is Nothing OrElse oOut Is Nothing Then Exit Sub

        Dim psTag As String = TryCast(oNode.Tag, String)

        If oNode.IsExpanded AndAlso Not String.IsNullOrWhiteSpace(psTag) AndAlso Directory.Exists(psTag) Then
            oOut.Add(psTag)
        End If

        For Each poChild As TreeNode In oNode.Nodes
            CollectExpandedFolderPathsRecursive(poChild, oOut)
        Next

    End Sub

    Private Function GetSelectedNodePath() As String

        If tvwFiles.SelectedNode Is Nothing Then Return String.Empty

        Dim psTag As String = TryCast(tvwFiles.SelectedNode.Tag, String)
        If String.IsNullOrWhiteSpace(psTag) Then Return String.Empty

        Return psTag

    End Function

    Private Function FindNodeByPath(sPath As String) As TreeNode

        If String.IsNullOrWhiteSpace(sPath) Then Return Nothing

        For Each poNode As TreeNode In tvwFiles.Nodes
            Dim poFound As TreeNode = FindNodeByPathRecursive(poNode, sPath)
            If poFound IsNot Nothing Then Return poFound
        Next

        Return Nothing

    End Function

    Private Function FindNodeByPathRecursive(oNode As TreeNode, sPath As String) As TreeNode

        If oNode Is Nothing Then Return Nothing

        Dim psTag As String = TryCast(oNode.Tag, String)

        If Not String.IsNullOrWhiteSpace(psTag) AndAlso String.Equals(psTag, sPath, StringComparison.OrdinalIgnoreCase) Then
            Return oNode
        End If

        For Each poChild As TreeNode In oNode.Nodes
            Dim poFound As TreeNode = FindNodeByPathRecursive(poChild, sPath)
            If poFound IsNot Nothing Then Return poFound
        Next

        Return Nothing

    End Function

    ' ---------- Tree search ----------
    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click

        Dim psSearch As String = txtSearch.Text.Trim()

        If psSearch = String.Empty Then
            System.Media.SystemSounds.Beep.Play()
            txtSearch.Focus()
            Exit Sub
        End If

        If tvwFiles.Nodes.Count = 0 Then
            System.Media.SystemSounds.Beep.Play()
            Exit Sub
        End If

        If Not String.Equals(fsLastSearchText, psSearch, StringComparison.OrdinalIgnoreCase) Then
            fsLastSearchText = psSearch
            foLastFoundNode = Nothing
        ElseIf foLastFoundNode IsNot Nothing AndAlso Not IsNodeStillInTree(foLastFoundNode) Then
            foLastFoundNode = Nothing
        End If

        Dim poFound As TreeNode = FindNextMatchingNode(psSearch)

        If poFound Is Nothing Then
            System.Media.SystemSounds.Beep.Play()
            MessageBox.Show(Me, "No matching node was found.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        SelectAndRevealNode(poFound)
        foLastFoundNode = poFound

    End Sub


    Private Sub ResetSearchState()

        fsLastSearchText = String.Empty
        foLastFoundNode = Nothing

    End Sub

    Private Function FindNextMatchingNode(sSearchText As String) As TreeNode

        Dim poFirstNode As TreeNode = GetFirstTreeNode()
        If poFirstNode Is Nothing Then Return Nothing

        Dim poCurrent As TreeNode

        If foLastFoundNode Is Nothing Then
            poCurrent = poFirstNode
        Else
            poCurrent = GetNextNodePreOrder(foLastFoundNode)

            If poCurrent Is Nothing Then
                poCurrent = poFirstNode
            End If
        End If

        Dim poStartNode As TreeNode = poCurrent

        Do

            If NodeMatchesSearch(poCurrent, sSearchText) Then
                Return poCurrent
            End If

            poCurrent = GetNextNodePreOrder(poCurrent)

            If poCurrent Is Nothing Then
                poCurrent = poFirstNode
            End If

        Loop While Not Object.ReferenceEquals(poCurrent, poStartNode)

        Return Nothing

    End Function

    Private Function GetFirstTreeNode() As TreeNode

        If tvwFiles.Nodes.Count = 0 Then Return Nothing
        Return tvwFiles.Nodes(0)

    End Function

    Private Function GetNextNodePreOrder(oNode As TreeNode) As TreeNode

        If oNode Is Nothing Then Return Nothing

        If oNode.Nodes.Count > 0 Then
            Return oNode.Nodes(0)
        End If

        Dim poCurrent As TreeNode = oNode

        While poCurrent IsNot Nothing

            Dim poParent As TreeNode = poCurrent.Parent
            Dim piIndex As Integer

            If poParent Is Nothing Then
                piIndex = poCurrent.Index

                If piIndex < tvwFiles.Nodes.Count - 1 Then
                    Return tvwFiles.Nodes(piIndex + 1)
                End If
            Else
                piIndex = poCurrent.Index

                If piIndex < poParent.Nodes.Count - 1 Then
                    Return poParent.Nodes(piIndex + 1)
                End If
            End If

            poCurrent = poParent

        End While

        Return Nothing

    End Function

    Private Function NodeMatchesSearch(oNode As TreeNode, sSearchText As String) As Boolean

        If oNode Is Nothing Then Return False
        If String.IsNullOrWhiteSpace(sSearchText) Then Return False

        Dim psNeedle As String = sSearchText.Trim()

        If oNode.Text.IndexOf(psNeedle, StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return True
        End If

        Dim psTag As String = TryCast(oNode.Tag, String)

        If Not String.IsNullOrWhiteSpace(psTag) Then
            If psTag.IndexOf(psNeedle, StringComparison.OrdinalIgnoreCase) >= 0 Then
                Return True
            End If
        End If

        Return False

    End Function

    Private Sub SelectAndRevealNode(oNode As TreeNode)

        If oNode Is Nothing Then Exit Sub

        tvwFiles.BeginUpdate()

        ExpandAncestors(oNode)
        tvwFiles.SelectedNode = oNode
        oNode.EnsureVisible()

        tvwFiles.EndUpdate()
        tvwFiles.Focus()

    End Sub

    Private Sub ExpandAncestors(oNode As TreeNode)

        Dim poStack As New Stack(Of TreeNode)()
        Dim poCurrent As TreeNode = oNode.Parent

        While poCurrent IsNot Nothing
            poStack.Push(poCurrent)
            poCurrent = poCurrent.Parent
        End While

        While poStack.Count > 0
            poStack.Pop().Expand()
        End While

    End Sub

    Private Function IsNodeStillInTree(oNode As TreeNode) As Boolean

        If oNode Is Nothing Then Return False

        Dim poCurrent As TreeNode = oNode

        While poCurrent.Parent IsNot Nothing
            poCurrent = poCurrent.Parent
        End While

        For Each poRoot As TreeNode In tvwFiles.Nodes
            If Object.ReferenceEquals(poRoot, poCurrent) Then
                Return True
            End If
        Next

        Return False

    End Function

    ' ---------- Export path browse ----------
    Private Sub btnBrowseExport_Click(sender As Object, e As EventArgs) Handles btnBrowseExport.Click

        Dim poDlg As New SaveFileDialog()
        poDlg.Title = "Export ZIP"
        poDlg.Filter = "ZIP files (*.zip)|*.zip|All files (*.*)|*.*"
        poDlg.DefaultExt = "zip"
        poDlg.AddExtension = True
        poDlg.OverwritePrompt = True

        If Not String.IsNullOrWhiteSpace(txtExportTo.Text) Then
            Try
                poDlg.InitialDirectory = Path.GetDirectoryName(txtExportTo.Text)
                poDlg.FileName = Path.GetFileName(txtExportTo.Text)
            Catch
                ' ignore
            End Try
        End If

        If poDlg.ShowDialog(Me) <> DialogResult.OK Then Exit Sub

        txtExportTo.Text = poDlg.FileName

        Dim psFolder As String = Path.GetDirectoryName(txtExportTo.Text)
        My.Settings.ExportFolder = psFolder
        My.Settings.Save()

    End Sub

    ' ---------- Export ----------
    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If String.IsNullOrWhiteSpace(fsUnityRoot) OrElse Not Directory.Exists(fsUnityRoot) Then
            MessageBox.Show(Me, "Select a Unity folder first.", "Missing Unity Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If String.IsNullOrWhiteSpace(fsAssetsRoot) OrElse Not Directory.Exists(fsAssetsRoot) Then
            MessageBox.Show(Me, "Assets folder was not found.", "Missing Assets Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If String.IsNullOrWhiteSpace(txtExportTo.Text) Then
            MessageBox.Show(Me, "Choose an Export To path first.", "Missing Export Path", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim poFiles As List(Of String) = GetCheckedFiles()
        Dim pbKeepJSON As Boolean = CBool(cboOutput.SelectedIndex = 1)

        If poFiles.Count = 0 Then
            MessageBox.Show(Me, "No supported files are checked.", "Nothing To Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Try
            Dim poExportFiles As List(Of String) = GetExportFiles(poFiles)

            Dim poPack As New UnityCsPack
            poPack.Format = "UnityCsPack.v2"
            poPack.UnityRoot = fsUnityRoot
            poPack.Root = fsAssetsRoot
            poPack.CreatedUtc = Date.UtcNow
            poPack.Files = New List(Of UnityCsFileEntry)

            For Each psFile As String In poExportFiles
                Dim psText = ReadFilePreserveFormatting(psFile)

                Dim poEntry As New UnityCsFileEntry
                poEntry.Path = MakeUnityRelativePath(psFile)
                poEntry.Sha256 = ComputeSha256Hex(psText)
                poEntry.Text = psText

                poPack.Files.Add(poEntry)
            Next

            Dim poJsonSettings As New JsonSerializerSettings
            poJsonSettings.Formatting = Formatting.None
            poJsonSettings.NullValueHandling = NullValueHandling.Ignore

            Dim psJson = JsonConvert.SerializeObject(poPack, poJsonSettings)

            Dim psExportFolder As String = txtExportTo.Text.Trim

            If psExportFolder.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) OrElse psExportFolder.EndsWith(".json", StringComparison.OrdinalIgnoreCase) Then
                psExportFolder = Path.GetDirectoryName(psExportFolder)
            End If

            If String.IsNullOrWhiteSpace(psExportFolder) Then
                Throw New InvalidOperationException("The export folder is invalid.")
            End If

            Directory.CreateDirectory(psExportFolder)

            For Each psOldCodepackFile As String In Directory.GetFiles(psExportFolder, "codepack*.*")
                File.Delete(psOldCodepackFile)
            Next

            Dim psTimestamp As String = Date.Now.ToString("yyMMddHHmmss")
            Dim psJsonFileName As String = $"codepack-{psTimestamp}.json"
            Dim psZipFileName As String = $"codepack-{psTimestamp}.zip"

            Dim psJsonPath As String = Path.Combine(psExportFolder, psJsonFileName)
            Dim psZipPath As String = Path.Combine(psExportFolder, psZipFileName)

            File.WriteAllText(psJsonPath, psJson, New UTF8Encoding(False))

            Using poArchive = ZipFile.Open(psZipPath, ZipArchiveMode.Create)
                poArchive.CreateEntryFromFile(psJsonPath, psJsonFileName, CompressionLevel.Optimal)
            End Using

            If Not pbKeepJSON Then
                File.Delete(psJsonPath)
            End If

            MessageBox.Show(Me, $"Exported {poExportFiles.Count} file(s) to ZIP.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
            OpenExplorerSelectFile(psZipPath)

        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function GetCheckedFiles() As List(Of String)

        Dim poList As New List(Of String)()

        If tvwFiles.Nodes.Count = 0 Then Return poList

        For Each poNode As TreeNode In tvwFiles.Nodes
            CollectCheckedFilesRecursive(poNode, poList)
        Next

        Return poList.Distinct(StringComparer.OrdinalIgnoreCase).ToList()

    End Function


    Private Function GetExportFiles(ByVal oSelectedFiles As IEnumerable(Of String)) As List(Of String)

        Dim poExportFiles As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        If oSelectedFiles Is Nothing Then Return poExportFiles.ToList()

        For Each psFile As String In oSelectedFiles

            If String.IsNullOrWhiteSpace(psFile) OrElse Not File.Exists(psFile) Then Continue For

            poExportFiles.Add(psFile)

            If Not fbIncludeMetaFiles Then Continue For

            ' Unity sidecar files retain the complete source filename, including its extension.
            ' Example: SomeFile.cs -> SomeFile.cs.meta
            Dim psMetaFile As String = psFile & ".meta"

            If File.Exists(psMetaFile) Then
                poExportFiles.Add(psMetaFile)
            End If

        Next

        Dim poSortedFiles As List(Of String) = poExportFiles.ToList()
        poSortedFiles.Sort(StringComparer.OrdinalIgnoreCase)
        Return poSortedFiles

    End Function

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click

        If String.IsNullOrWhiteSpace(fsAssetsRoot) OrElse Not Directory.Exists(fsAssetsRoot) Then
            System.Media.SystemSounds.Beep.Play()
            ClearAssetsTreeAndSelectedList()
            Exit Sub
        End If

        Dim poCheckedFiles As New HashSet(Of String)(GetCheckedFiles(), StringComparer.OrdinalIgnoreCase)
        Dim poExpandedFolders As HashSet(Of String) = GetExpandedFolderPaths()
        Dim psSelectedPath As String = GetSelectedNodePath()

        BuildTree(poCheckedFiles, poExpandedFolders, psSelectedPath)

    End Sub

    Private Sub txtSearch_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtSearch.KeyPress
        If e.KeyChar = ControlChars.Cr Then
            e.Handled = True
            btnSearch_Click(Nothing, Nothing)
        End If
    End Sub


    Private Sub txtSearch_GotFocus(sender As Object, e As EventArgs) Handles txtSearch.GotFocus
        txtSearch.SelectAll()
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        ResetSearchState()
    End Sub





    Private Sub CollectCheckedFilesRecursive(oNode As TreeNode, oOut As List(Of String))

        If oNode.Checked AndAlso oNode.Tag IsNot Nothing Then

            Dim psTag As String = TryCast(oNode.Tag, String)

            If Not String.IsNullOrWhiteSpace(psTag) AndAlso File.Exists(psTag) AndAlso IsAllowedFile(psTag) Then
                oOut.Add(psTag)
            End If

        End If

        For Each poChild As TreeNode In oNode.Nodes
            CollectCheckedFilesRecursive(poChild, oOut)
        Next

    End Sub

    Private Function MakeUnityRelativePath(sFullPath As String) As String

        Dim psFull As String = Path.GetFullPath(sFullPath)
        Dim psAssets As String = Path.GetFullPath(fsAssetsRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        Dim psAssetsPrefix As String = psAssets & Path.DirectorySeparatorChar

        If String.Equals(psFull, psAssets, StringComparison.OrdinalIgnoreCase) Then
            Return "Assets"
        End If

        If psFull.StartsWith(psAssetsPrefix, StringComparison.OrdinalIgnoreCase) Then
            Dim psRel As String = "Assets" & psFull.Substring(psAssets.Length)
            Return psRel.Replace("\", "/")
        End If

        Dim psRoot As String = Path.GetFullPath(fsUnityRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) & Path.DirectorySeparatorChar

        If psFull.StartsWith(psRoot, StringComparison.OrdinalIgnoreCase) Then
            Return psFull.Substring(psRoot.Length).Replace("\", "/")
        End If

        Return psFull.Replace("\", "/")

    End Function

    Private Function ReadFilePreserveFormatting(sFilePath As String) As String

        ' This reads the text without altering indentation or line endings.
        ' StreamReader with detectEncodingFromByteOrderMarks:=True preserves the actual text content as stored.
        Using poReader As New StreamReader(sFilePath, Encoding.UTF8, True)
            Return poReader.ReadToEnd()
        End Using

    End Function

    Private Function ComputeSha256Hex(sText As String) As String

        Dim pyBytes() As Byte = Encoding.UTF8.GetBytes(sText)

        Using poSha As SHA256 = SHA256.Create()

            Dim pyHash() As Byte = poSha.ComputeHash(pyBytes)
            Dim poSb As New StringBuilder(pyHash.Length * 2)

            For Each pyB As Byte In pyHash
                poSb.Append(pyB.ToString("x2"))
            Next

            Return poSb.ToString()

        End Using

    End Function

    Private Sub OpenExplorerSelectFile(ByVal sFilePath As String)
        Try
            If String.IsNullOrWhiteSpace(sFilePath) Then
                Return
            End If

            If Not File.Exists(sFilePath) Then
                MessageBox.Show("File not found:" & Environment.NewLine & sFilePath, "Open File Location", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim psArguments As String = "/select,""" & sFilePath & """"
            Dim poProcess As Process = Process.Start("explorer.exe", psArguments)

            BringExplorerWindowToFront(poProcess, sFilePath)

        Catch poEx As Exception
            MessageBox.Show("Unable to open file location:" & Environment.NewLine & poEx.Message, "Open File Location", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BringExplorerWindowToFront(ByVal oExplorerProcess As Process, ByVal sFilePath As String)
        Try
            Thread.Sleep(350)

            If oExplorerProcess IsNot Nothing Then
                oExplorerProcess.Refresh()

                If oExplorerProcess.MainWindowHandle <> IntPtr.Zero Then
                    ShowWindowAsync(oExplorerProcess.MainWindowHandle, miSW_RESTORE)
                    SetForegroundWindow(oExplorerProcess.MainWindowHandle)
                    Return
                End If
            End If

            Dim psDirectory As String = Path.GetDirectoryName(sFilePath)

            For Each poProcess As Process In Process.GetProcessesByName("explorer")
                Try
                    If poProcess.MainWindowHandle <> IntPtr.Zero Then
                        ShowWindowAsync(poProcess.MainWindowHandle, miSW_RESTORE)
                        SetForegroundWindow(poProcess.MainWindowHandle)
                        Return
                    End If

                Catch
                    ' Ignore individual Explorer process access issues.
                End Try
            Next

        Catch
            ' Explorer opened successfully; foreground promotion is best-effort only.
        End Try
    End Sub

    ' ---------- JSON models ----------
    Private Class UnityCsPack
        Public Property Format As String
        Public Property UnityRoot As String
        Public Property Root As String
        Public Property CreatedUtc As Date
        Public Property Files As List(Of UnityCsFileEntry)
    End Class

    Private Class UnityCsFileEntry
        Public Property Path As String
        Public Property Sha256 As String
        Public Property Text As String
    End Class


End Class