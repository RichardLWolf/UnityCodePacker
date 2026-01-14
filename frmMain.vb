Option Strict On
Option Explicit On

Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json

Public Class frmMain

    ' --- icons ---
    Private Const ksIconFolderClosed As String = "folder_closed"
    Private Const ksIconFolderOpen As String = "folder_open"
    Private Const ksIconCsFile As String = "c_file"


    ' --- state ---
    Private fsUnityRoot As String = String.Empty
    Private fsScriptsRoot As String = String.Empty
    Private fbTreeBusy As Boolean = False

    ' ---------- Form ----------
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim psUnity As String = My.Settings.UnityFolder
        Dim psExport As String = My.Settings.ExportFolder

        If Not String.IsNullOrWhiteSpace(psUnity) AndAlso Directory.Exists(psUnity) Then
            fsUnityRoot = psUnity
            fsScriptsRoot = Path.Combine(fsUnityRoot, "Assets", "Scripts")
            lblUnityFolder.Text = "Unity Folder: " & fsUnityRoot
            lblUnityFolder.ToolTipText = fsScriptsRoot


            If Directory.Exists(fsScriptsRoot) Then
                BuildTree()
            End If
        Else
            tvwFiles.Nodes.Clear()
        End If

        If Not String.IsNullOrWhiteSpace(psExport) AndAlso Directory.Exists(psExport) Then
            txtExportTo.Text = Path.Combine(psExport, "codepack.json")
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
        fsScriptsRoot = Path.Combine(fsUnityRoot, "Assets", "Scripts")

        lblUnityFolder.Text = "Unity Folder: " & fsUnityRoot
        lblUnityFolder.ToolTipText = fsScriptsRoot


        If Not Directory.Exists(Path.Combine(fsUnityRoot, "Assets")) Then
            MessageBox.Show(Me, "That folder doesn’t contain an Assets\ folder. Pick the Unity project root.", "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            fsUnityRoot = ""
            fsScriptsRoot = ""
            tvwFiles.Nodes.Clear()
            Exit Sub
        End If

        If Not Directory.Exists(fsScriptsRoot) Then
            MessageBox.Show(Me, "Assets\Scripts not found. Creating tree from Assets\ instead.", "Scripts Folder Missing", MessageBoxButtons.OK, MessageBoxIcon.Information)
            fsScriptsRoot = Path.Combine(fsUnityRoot, "Assets")
        End If

        My.Settings.UnityFolder = fsUnityRoot
        My.Settings.Save()

        BuildTree()
    End Sub

    Private Sub BuildTree()
        If String.IsNullOrWhiteSpace(fsScriptsRoot) OrElse Not Directory.Exists(fsScriptsRoot) Then
            tvwFiles.Nodes.Clear()
            Exit Sub
        End If

        tvwFiles.BeginUpdate()
        tvwFiles.Nodes.Clear()

        Dim poRootNode As TreeNode = CreateFolderNode(fsScriptsRoot)
        poRootNode.Expand()
        tvwFiles.Nodes.Add(poRootNode)

        tvwFiles.EndUpdate()
    End Sub

    ' Folders are nodes; only *.cs files become leaf nodes (Tag = full file path)
    Private Function CreateFolderNode(sFolder As String) As TreeNode
        Dim psName As String = Path.GetFileName(sFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        If String.IsNullOrWhiteSpace(psName) Then psName = sFolder

        Dim poNode As New TreeNode(psName)
        poNode.Tag = sFolder
        poNode.ImageKey = ksIconFolderClosed
        poNode.SelectedImageKey = ksIconFolderOpen

        ' Add subfolders (recursive)
        Dim paDirs() As String = {}
        Try
            paDirs = Directory.GetDirectories(sFolder)
        Catch
            paDirs = {}
        End Try

        Array.Sort(paDirs, StringComparer.OrdinalIgnoreCase)
        For Each psDir As String In paDirs
            ' Skip Unity junk if user chose root above Assets accidentally, or if Assets contains oddities
            Dim psLower As String = psDir.ToLowerInvariant()
            If psLower.Contains(Path.DirectorySeparatorChar & "library") OrElse
               psLower.Contains(Path.DirectorySeparatorChar & "temp") OrElse
               psLower.Contains(Path.DirectorySeparatorChar & "obj") OrElse
               psLower.Contains(Path.DirectorySeparatorChar & "logs") OrElse
               psLower.Contains(Path.DirectorySeparatorChar & ".git") OrElse
               psLower.Contains(Path.DirectorySeparatorChar & "builds") Then
                Continue For
            End If

            poNode.Nodes.Add(CreateFolderNode(psDir))
        Next

        ' Add *.cs files
        Dim paFiles() As String = {}
        Try
            paFiles = Directory.GetFiles(sFolder, "*.cs", SearchOption.TopDirectoryOnly)
        Catch
            paFiles = {}
        End Try

        Array.Sort(paFiles, StringComparer.OrdinalIgnoreCase)
        For Each psFile As String In paFiles
            Dim poFileNode As New TreeNode(Path.GetFileName(psFile))
            poFileNode.Tag = psFile
            poFileNode.ImageKey = ksIconCsFile
            poFileNode.SelectedImageKey = ksIconCsFile
            poNode.Nodes.Add(poFileNode)
        Next

        Return poNode
    End Function

    ' ---------- Select all / none ----------
    Private Sub btnSelectAll_Click(sender As Object, e As EventArgs) Handles btnSelectAll.Click
        SetAllChecks(True)
    End Sub

    Private Sub btnSelectNone_Click(sender As Object, e As EventArgs) Handles btnSelectNone.Click
        SetAllChecks(False)
    End Sub

    Private Sub SetAllChecks(bChecked As Boolean)
        If tvwFiles.Nodes.Count = 0 Then Exit Sub

        fbTreeBusy = True
        tvwFiles.BeginUpdate()

        For Each poNode As TreeNode In tvwFiles.Nodes
            SetNodeCheckedRecursive(poNode, bChecked)
        Next

        tvwFiles.EndUpdate()
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

        ' 1) Cascade to children
        For Each poChild As TreeNode In e.Node.Nodes
            SetNodeCheckedRecursive(poChild, e.Node.Checked)
        Next

        ' 2) Bubble up: parent checked if any child checked; otherwise unchecked
        UpdateParentsFromChildren(e.Node)

        fbTreeBusy = False
    End Sub

    Private Sub tvwFiles_AfterExpand(sender As Object, e As TreeViewEventArgs) Handles tvwFiles.AfterExpand
        If IsFolderNode(e.Node) Then e.Node.ImageKey = ksIconFolderOpen
    End Sub

    Private Sub tvwFiles_AfterCollapse(sender As Object, e As TreeViewEventArgs) Handles tvwFiles.AfterCollapse
        If IsFolderNode(e.Node) Then e.Node.ImageKey = ksIconFolderClosed
    End Sub

    Private Function IsFolderNode(oNode As TreeNode) As Boolean
        Dim psTag As String = TryCast(oNode.Tag, String)
        If String.IsNullOrWhiteSpace(psTag) Then Return False
        Return Directory.Exists(psTag)
    End Function

    Private Sub tvwFiles_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvwFiles.NodeMouseClick
        If e.Node.Nodes.Count > 0 Then
            If e.Node.IsExpanded Then
                e.Node.Collapse()
            Else
                e.Node.Expand()
            End If
        End If
    End Sub



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

    ' ---------- Export path browse ----------
    Private Sub btnBrowseExport_Click(sender As Object, e As EventArgs) Handles btnBrowseExport.Click
        Dim poDlg As New SaveFileDialog()
        poDlg.Title = "Export JSON"
        poDlg.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        poDlg.DefaultExt = "json"
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

        If String.IsNullOrWhiteSpace(txtExportTo.Text) Then
            MessageBox.Show(Me, "Choose an Export To path first.", "Missing Export Path", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim poFiles As List(Of String) = GetCheckedCsFiles()
        If poFiles.Count = 0 Then
            MessageBox.Show(Me, "No .cs files are checked.", "Nothing To Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Try
            Dim poPack As New UnityCsPack()
            poPack.Format = "UnityCsPack.v1"
            poPack.UnityRoot = fsUnityRoot
            poPack.Root = fsScriptsRoot
            poPack.CreatedUtc = Date.UtcNow

            poPack.Files = New List(Of UnityCsFileEntry)()

            For Each psFile As String In poFiles
                Dim psText As String = File.ReadAllText(psFile, Encoding.UTF8)

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

            Dim psOutPath As String = txtExportTo.Text.Trim()
            Directory.CreateDirectory(Path.GetDirectoryName(psOutPath))
            File.WriteAllText(psOutPath, psJson, New UTF8Encoding(False))

            MessageBox.Show(Me, $"Exported {poFiles.Count} file(s).", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function GetCheckedCsFiles() As List(Of String)
        Dim poList As New List(Of String)()

        If tvwFiles.Nodes.Count = 0 Then Return poList

        For Each poNode As TreeNode In tvwFiles.Nodes
            CollectCheckedFiles(poNode, poList)
        Next

        ' De-dupe just in case
        poList = poList.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        Return poList
    End Function

    Private Sub CollectCheckedFiles(oNode As TreeNode, oOut As List(Of String))
        ' File nodes have Tag = full path and end with .cs
        If oNode.Checked AndAlso oNode.Tag IsNot Nothing Then
            Dim psTag As String = TryCast(oNode.Tag, String)
            If Not String.IsNullOrWhiteSpace(psTag) AndAlso psTag.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) Then
                oOut.Add(psTag)
            End If
        End If

        For Each poChild As TreeNode In oNode.Nodes
            CollectCheckedFiles(poChild, oOut)
        Next
    End Sub

    Private Function MakeUnityRelativePath(sFullPath As String) As String
        ' Prefer "Assets/..." style paths
        Dim psFull As String = Path.GetFullPath(sFullPath)
        Dim psAssets As String = Path.Combine(fsUnityRoot, "Assets")
        psAssets = Path.GetFullPath(psAssets)

        If psFull.StartsWith(psAssets, StringComparison.OrdinalIgnoreCase) Then
            Dim psRel As String = "Assets" & psFull.Substring(psAssets.Length)
            Return psRel.Replace("\", "/")
        End If

        ' Fallback: relative to unity root
        Dim psRoot As String = Path.GetFullPath(fsUnityRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) & Path.DirectorySeparatorChar
        If psFull.StartsWith(psRoot, StringComparison.OrdinalIgnoreCase) Then
            Return psFull.Substring(psRoot.Length).Replace("\", "/")
        End If

        Return psFull.Replace("\", "/")
    End Function

    Private Function ComputeSha256Hex(sText As String) As String
        Dim pyBytes() As Byte = Encoding.UTF8.GetBytes(sText)
        Using poSha As SHA256 = SHA256.Create()
            Dim pyHash() As Byte = poSha.ComputeHash(pyBytes)
            Dim poSb As New StringBuilder(pyHash.Length * 2)
            For Each byB As Byte In pyHash
                poSb.Append(byB.ToString("x2"))
            Next
            Return poSb.ToString()
        End Using
    End Function

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
