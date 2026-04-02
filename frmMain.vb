Option Strict On
Option Explicit On

Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json

Public Class frmMain

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
            ".md",
            ".xml",
            ".yml",
            ".yaml"
        }

    ' ---------- Form ----------
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim psUnity As String = My.Settings.UnityFolder
        Dim psExport As String = My.Settings.ExportFolder

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
                tvwFiles.Nodes.Clear()
                ResetSearchState()
            End If

        Else
            tvwFiles.Nodes.Clear()
            ResetSearchState()
        End If

        If Not String.IsNullOrWhiteSpace(psExport) AndAlso Directory.Exists(psExport) Then
            txtExportTo.Text = Path.Combine(psExport, "codepack.zip")
        End If
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
            tvwFiles.Nodes.Clear()
            ResetSearchState()
            Exit Sub
        End If

        lblUnityFolder.Text = "Unity Folder: " & fsUnityRoot
        lblUnityFolder.ToolTipText = fsAssetsRoot

        My.Settings.UnityFolder = fsUnityRoot
        My.Settings.Save()

        BuildTree()

    End Sub

    Private Sub BuildTree()

        ResetSearchState()

        If String.IsNullOrWhiteSpace(fsAssetsRoot) OrElse Not Directory.Exists(fsAssetsRoot) Then
            tvwFiles.Nodes.Clear()
            Exit Sub
        End If

        tvwFiles.BeginUpdate()
        tvwFiles.Nodes.Clear()

        Dim poRootNode As TreeNode = CreateFolderNode(fsAssetsRoot)
        poRootNode.Expand()
        tvwFiles.Nodes.Add(poRootNode)

        tvwFiles.EndUpdate()

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
            Case "library", "temp", "obj", "logs", ".git", "builds", "packagesettings", "usersettings"
                Return True
        End Select

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

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        ResetSearchState()
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

        If poFiles.Count = 0 Then
            MessageBox.Show(Me, "No supported files are checked.", "Nothing To Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Try

            Dim poPack As New UnityCsPack()
            poPack.Format = "UnityCsPack.v2"
            poPack.UnityRoot = fsUnityRoot
            poPack.Root = fsAssetsRoot
            poPack.CreatedUtc = Date.UtcNow
            poPack.Files = New List(Of UnityCsFileEntry)()

            For Each psFile As String In poFiles

                Dim psText As String = ReadFilePreserveFormatting(psFile)

                Dim poEntry As New UnityCsFileEntry()
                poEntry.Path = MakeUnityRelativePath(psFile)
                poEntry.Sha256 = ComputeSha256Hex(psText)
                poEntry.Text = psText

                poPack.Files.Add(poEntry)

            Next

            Dim poJsonSettings As New JsonSerializerSettings()
            poJsonSettings.Formatting = Formatting.None
            poJsonSettings.NullValueHandling = NullValueHandling.Ignore

            Dim psJson As String = JsonConvert.SerializeObject(poPack, poJsonSettings)

            Dim psZipPath As String = txtExportTo.Text.Trim()

            If Not psZipPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) Then
                psZipPath &= ".zip"
            End If

            Dim psOutFolder As String = Path.GetDirectoryName(psZipPath)
            If String.IsNullOrWhiteSpace(psOutFolder) Then
                Throw New InvalidOperationException("The export folder is invalid.")
            End If

            Directory.CreateDirectory(psOutFolder)

            Dim psJsonPath As String = Path.Combine(psOutFolder, "codepack.json")

            If File.Exists(psJsonPath) Then File.Delete(psJsonPath)
            If File.Exists(psZipPath) Then File.Delete(psZipPath)

            File.WriteAllText(psJsonPath, psJson, New UTF8Encoding(False))

            Using poArchive As ZipArchive = ZipFile.Open(psZipPath, ZipArchiveMode.Create)
                poArchive.CreateEntryFromFile(psJsonPath, "codepack.json", CompressionLevel.Optimal)
            End Using

            File.Delete(psJsonPath)

            MessageBox.Show(Me, $"Exported {poFiles.Count} file(s) to ZIP.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
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

    Public Sub OpenExplorerSelectFile(sTargetPath As String)

        If Not File.Exists(sTargetPath) Then Exit Sub

        Dim psArgs As String = "/select,""" & sTargetPath & """"
        Process.Start("explorer.exe", psArgs)

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

    Private Sub txtSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.Handled = True
            btnSearch_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub txtSearch_GotFocus(sender As Object, e As EventArgs) Handles txtSearch.GotFocus
        txtSearch.SelectionStart = 0
        txtSearch.SelectionLength = txtSearch.Text.Length
    End Sub
End Class