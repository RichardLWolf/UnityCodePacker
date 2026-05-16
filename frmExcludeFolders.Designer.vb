<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmExcludeFolders
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmExcludeFolders))
        tvwAssetFolder = New TreeView()
        btnOk = New Button()
        btnCancel = New Button()
        btnClearAllChecks = New Button()
        SuspendLayout()
        ' 
        ' tvwAssetFolder
        ' 
        tvwAssetFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        tvwAssetFolder.Font = New Font("Segoe UI", 12F)
        tvwAssetFolder.Location = New Point(12, 12)
        tvwAssetFolder.Name = "tvwAssetFolder"
        tvwAssetFolder.Size = New Size(776, 398)
        tvwAssetFolder.TabIndex = 0
        ' 
        ' btnOk
        ' 
        btnOk.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnOk.DialogResult = DialogResult.OK
        btnOk.Font = New Font("Segoe UI", 12F)
        btnOk.Location = New Point(713, 416)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(75, 30)
        btnOk.TabIndex = 1
        btnOk.Text = "OK"
        btnOk.UseVisualStyleBackColor = True
        ' 
        ' btnCancel
        ' 
        btnCancel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Font = New Font("Segoe UI", 12F)
        btnCancel.Location = New Point(632, 416)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(75, 30)
        btnCancel.TabIndex = 2
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' btnClearAllChecks
        ' 
        btnClearAllChecks.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnClearAllChecks.Font = New Font("Segoe UI", 12F)
        btnClearAllChecks.Location = New Point(12, 416)
        btnClearAllChecks.Name = "btnClearAllChecks"
        btnClearAllChecks.Size = New Size(95, 30)
        btnClearAllChecks.TabIndex = 3
        btnClearAllChecks.Text = "Clear All"
        btnClearAllChecks.UseVisualStyleBackColor = True
        ' 
        ' frmExcludeFolders
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(btnClearAllChecks)
        Controls.Add(btnCancel)
        Controls.Add(btnOk)
        Controls.Add(tvwAssetFolder)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "frmExcludeFolders"
        Text = "Excluded Folders List"
        ResumeLayout(False)
    End Sub

    Friend WithEvents tvwAssetFolder As TreeView
    Friend WithEvents btnOk As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnClearAllChecks As Button
End Class
