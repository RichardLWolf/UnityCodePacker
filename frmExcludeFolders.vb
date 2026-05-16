Option Strict On
Option Explicit On

Imports System.IO
Public Class frmExcludeFolders

    Private fsAssetFolderRoot As String = String.Empty
    Private foCurrentList As New List(Of String)
    Private fbTreeBusy As Boolean = False

    Public Sub Initialize(ByVal sAssetFolderRoot As String, ByVal oCurrentList As List(Of String))

        fsAssetFolderRoot = sAssetFolderRoot
        foCurrentList = NormalizeFolderList(oCurrentList)

        tvwAssetFolder.CheckBoxes = True
        LoadFolderTree()

    End Sub

    Public ReadOnly Property SelectedFolderList As List(Of String)
        Get
            Return NormalizeFolderList(foCurrentList)
        End Get
    End Property

    Private Sub LoadFolderTree()

        fbTreeBusy = True
        tvwAssetFolder.BeginUpdate()

        Try
            tvwAssetFolder.Nodes.Clear()

            If String.IsNullOrWhiteSpace(fsAssetFolderRoot) OrElse Not Directory.Exists(fsAssetFolderRoot) Then
                Exit Sub
            End If

            Dim poRootNode As TreeNode = CreateFolderNode(fsAssetFolderRoot, True)
            tvwAssetFolder.Nodes.Add(poRootNode)
            poRootNode.Expand()

        Finally
            tvwAssetFolder.EndUpdate()
            fbTreeBusy = False
        End Try

    End Sub

    Private Function CreateFolderNode(ByVal sFolder As String, ByVal bIsRoot As Boolean) As TreeNode

        Dim psText As String

        If bIsRoot Then
            psText = "Assets"
        Else
            psText = Path.GetFileName(sFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        End If

        If String.IsNullOrWhiteSpace(psText) Then psText = sFolder

        Dim poNode As New TreeNode(psText)
        Dim psRelativeFolder As String = GetAssetRelativeFolderPath(sFolder)

        poNode.Tag = psRelativeFolder
        poNode.Checked = Not String.IsNullOrWhiteSpace(psRelativeFolder) AndAlso FolderIsSelected(psRelativeFolder)

        Dim paDirs() As String = Array.Empty(Of String)()

        Try
            paDirs = Directory.GetDirectories(sFolder)
        Catch
            paDirs = Array.Empty(Of String)()
        End Try

        Array.Sort(paDirs, StringComparer.OrdinalIgnoreCase)

        For Each psDir As String In paDirs
            poNode.Nodes.Add(CreateFolderNode(psDir, False))
        Next

        Return poNode

    End Function

    Private Function FolderIsSelected(ByVal sRelativeFolder As String) As Boolean

        For Each psFolder As String In foCurrentList
            If String.Equals(psFolder, sRelativeFolder, StringComparison.OrdinalIgnoreCase) Then Return True
        Next

        Return False

    End Function

    Private Sub btnClearAllChecks_Click(sender As Object, e As EventArgs) Handles btnClearAllChecks.Click

        foCurrentList.Clear()

        fbTreeBusy = True
        tvwAssetFolder.BeginUpdate()

        Try
            For Each poNode As TreeNode In tvwAssetFolder.Nodes
                SetNodeCheckedRecursive(poNode, False)
            Next
        Finally
            tvwAssetFolder.EndUpdate()
            fbTreeBusy = False
        End Try

    End Sub

    Private Sub SetNodeCheckedRecursive(ByVal oNode As TreeNode, ByVal bChecked As Boolean)

        If oNode Is Nothing Then Exit Sub

        oNode.Checked = bChecked

        For Each poChild As TreeNode In oNode.Nodes
            SetNodeCheckedRecursive(poChild, bChecked)
        Next

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
    End Sub

    Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click
        foCurrentList = NormalizeFolderList(foCurrentList)
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub tvwAssetFolder_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles tvwAssetFolder.AfterCheck

        If fbTreeBusy Then Exit Sub
        If e.Node Is Nothing Then Exit Sub

        Dim psRelativeFolder As String = TryCast(e.Node.Tag, String)

        If String.IsNullOrWhiteSpace(psRelativeFolder) Then
            fbTreeBusy = True
            e.Node.Checked = False
            fbTreeBusy = False
            Exit Sub
        End If

        If e.Node.Checked Then
            AddFolderToCurrentList(psRelativeFolder)
        Else
            RemoveFolderFromCurrentList(psRelativeFolder)
        End If

    End Sub

    Private Sub tvwAssetFolder_NodeMouseClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles tvwAssetFolder.NodeMouseClick

        If e.Node Is Nothing Then Exit Sub
        tvwAssetFolder.SelectedNode = e.Node

        If e.Node.Bounds.Contains(e.Location) AndAlso e.Node.Nodes.Count > 0 Then
            If e.Node.IsExpanded Then
                e.Node.Collapse()
            Else
                e.Node.Expand()
            End If
        End If

    End Sub

    Private Sub AddFolderToCurrentList(ByVal sRelativeFolder As String)

        Dim psRelativeFolder As String = NormalizeAssetRelativeFolder(sRelativeFolder)

        If String.IsNullOrWhiteSpace(psRelativeFolder) Then Exit Sub

        foCurrentList = NormalizeFolderList(foCurrentList)

        If Not CurrentListContainsFolder(psRelativeFolder) Then
            foCurrentList.Add(psRelativeFolder)
            foCurrentList.Sort(StringComparer.OrdinalIgnoreCase)
        End If

    End Sub

    Private Sub RemoveFolderFromCurrentList(ByVal sRelativeFolder As String)

        Dim psRelativeFolder As String = NormalizeAssetRelativeFolder(sRelativeFolder)

        If String.IsNullOrWhiteSpace(psRelativeFolder) Then Exit Sub

        For piIndex As Integer = foCurrentList.Count - 1 To 0 Step -1
            If String.Equals(foCurrentList(piIndex), psRelativeFolder, StringComparison.OrdinalIgnoreCase) Then
                foCurrentList.RemoveAt(piIndex)
            End If
        Next

    End Sub

    Private Function CurrentListContainsFolder(ByVal sRelativeFolder As String) As Boolean

        For Each psExistingFolder As String In foCurrentList
            If String.Equals(psExistingFolder, sRelativeFolder, StringComparison.OrdinalIgnoreCase) Then Return True
        Next

        Return False

    End Function

    Private Function NormalizeFolderList(ByVal oFolderList As IEnumerable(Of String)) As List(Of String)

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
        If String.IsNullOrWhiteSpace(fsAssetFolderRoot) Then Return String.Empty

        Try
            Dim psFull As String = Path.GetFullPath(sFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            Dim psAssets As String = Path.GetFullPath(fsAssetFolderRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
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

End Class
