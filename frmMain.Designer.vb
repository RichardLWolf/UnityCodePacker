<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        tspMenu = New ToolStrip()
        btnSelectAll = New ToolStripButton()
        btnSelectNone = New ToolStripButton()
        ToolStripSeparator1 = New ToolStripSeparator()
        ToolStripLabel1 = New ToolStripLabel()
        txtExportTo = New ToolStripTextBox()
        btnBrowseExport = New ToolStripButton()
        btnExport = New ToolStripButton()
        btnSelectUnity = New ToolStripButton()
        lblUnityFolder = New ToolStripLabel()
        tvwFiles = New TreeView()
        imlIcons = New ImageList(components)
        tspMenu.SuspendLayout()
        SuspendLayout()
        ' 
        ' tspMenu
        ' 
        tspMenu.BackColor = SystemColors.ControlDark
        tspMenu.GripStyle = ToolStripGripStyle.Hidden
        tspMenu.ImageScalingSize = New Size(32, 32)
        tspMenu.Items.AddRange(New ToolStripItem() {btnSelectAll, btnSelectNone, ToolStripSeparator1, ToolStripLabel1, txtExportTo, btnBrowseExport, btnExport, btnSelectUnity, lblUnityFolder})
        tspMenu.Location = New Point(0, 0)
        tspMenu.Name = "tspMenu"
        tspMenu.Padding = New Padding(10, 0, 10, 0)
        tspMenu.Size = New Size(896, 39)
        tspMenu.TabIndex = 0
        tspMenu.Text = "ToolStrip1"
        ' 
        ' btnSelectAll
        ' 
        btnSelectAll.DisplayStyle = ToolStripItemDisplayStyle.Image
        btnSelectAll.Image = CType(resources.GetObject("btnSelectAll.Image"), Image)
        btnSelectAll.ImageTransparentColor = Color.Magenta
        btnSelectAll.Margin = New Padding(0, 1, 5, 2)
        btnSelectAll.Name = "btnSelectAll"
        btnSelectAll.Size = New Size(36, 36)
        btnSelectAll.Text = "Select All"
        btnSelectAll.TextAlign = ContentAlignment.BottomCenter
        btnSelectAll.ToolTipText = "Select All"
        ' 
        ' btnSelectNone
        ' 
        btnSelectNone.DisplayStyle = ToolStripItemDisplayStyle.Image
        btnSelectNone.Image = CType(resources.GetObject("btnSelectNone.Image"), Image)
        btnSelectNone.ImageTransparentColor = Color.Magenta
        btnSelectNone.Margin = New Padding(0, 1, 5, 2)
        btnSelectNone.Name = "btnSelectNone"
        btnSelectNone.Size = New Size(36, 36)
        btnSelectNone.Text = "Select none"
        btnSelectNone.TextAlign = ContentAlignment.BottomCenter
        btnSelectNone.ToolTipText = "Select None"
        ' 
        ' ToolStripSeparator1
        ' 
        ToolStripSeparator1.Margin = New Padding(5, 0, 5, 0)
        ToolStripSeparator1.Name = "ToolStripSeparator1"
        ToolStripSeparator1.Size = New Size(6, 39)
        ' 
        ' ToolStripLabel1
        ' 
        ToolStripLabel1.ForeColor = Color.AntiqueWhite
        ToolStripLabel1.Name = "ToolStripLabel1"
        ToolStripLabel1.Size = New Size(56, 36)
        ToolStripLabel1.Text = "Export To"
        ' 
        ' txtExportTo
        ' 
        txtExportTo.Name = "txtExportTo"
        txtExportTo.Size = New Size(200, 39)
        ' 
        ' btnBrowseExport
        ' 
        btnBrowseExport.DisplayStyle = ToolStripItemDisplayStyle.Image
        btnBrowseExport.Image = CType(resources.GetObject("btnBrowseExport.Image"), Image)
        btnBrowseExport.ImageTransparentColor = Color.Magenta
        btnBrowseExport.Name = "btnBrowseExport"
        btnBrowseExport.Size = New Size(36, 36)
        btnBrowseExport.Text = "Browse folder"
        btnBrowseExport.TextImageRelation = TextImageRelation.ImageAboveText
        ' 
        ' btnExport
        ' 
        btnExport.DisplayStyle = ToolStripItemDisplayStyle.Image
        btnExport.Image = CType(resources.GetObject("btnExport.Image"), Image)
        btnExport.ImageTransparentColor = Color.Magenta
        btnExport.Name = "btnExport"
        btnExport.Size = New Size(36, 36)
        btnExport.Text = "Export JSON"
        btnExport.TextImageRelation = TextImageRelation.ImageAboveText
        ' 
        ' btnSelectUnity
        ' 
        btnSelectUnity.Alignment = ToolStripItemAlignment.Right
        btnSelectUnity.DisplayStyle = ToolStripItemDisplayStyle.Image
        btnSelectUnity.Image = CType(resources.GetObject("btnSelectUnity.Image"), Image)
        btnSelectUnity.ImageTransparentColor = Color.Magenta
        btnSelectUnity.Name = "btnSelectUnity"
        btnSelectUnity.Size = New Size(36, 36)
        btnSelectUnity.Text = "Select Unity Folder"
        ' 
        ' lblUnityFolder
        ' 
        lblUnityFolder.Alignment = ToolStripItemAlignment.Right
        lblUnityFolder.ForeColor = Color.Blue
        lblUnityFolder.Name = "lblUnityFolder"
        lblUnityFolder.Size = New Size(105, 36)
        lblUnityFolder.Text = "Select Unity Folder"
        lblUnityFolder.ToolTipText = "Selected Unity folder root."
        ' 
        ' tvwFiles
        ' 
        tvwFiles.CheckBoxes = True
        tvwFiles.Dock = DockStyle.Fill
        tvwFiles.Font = New Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tvwFiles.ImageIndex = 0
        tvwFiles.ImageList = imlIcons
        tvwFiles.Location = New Point(0, 39)
        tvwFiles.Name = "tvwFiles"
        tvwFiles.SelectedImageIndex = 0
        tvwFiles.Size = New Size(896, 411)
        tvwFiles.TabIndex = 1
        ' 
        ' imlIcons
        ' 
        imlIcons.ColorDepth = ColorDepth.Depth32Bit
        imlIcons.ImageStream = CType(resources.GetObject("imlIcons.ImageStream"), ImageListStreamer)
        imlIcons.TransparentColor = Color.Transparent
        imlIcons.Images.SetKeyName(0, "folder_closed")
        imlIcons.Images.SetKeyName(1, "folder_open")
        imlIcons.Images.SetKeyName(2, "c_file")
        ' 
        ' frmMain
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(896, 450)
        Controls.Add(tvwFiles)
        Controls.Add(tspMenu)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "frmMain"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Unity Code Packer"
        tspMenu.ResumeLayout(False)
        tspMenu.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tspMenu As ToolStrip
    Friend WithEvents btnSelectAll As ToolStripButton
    Friend WithEvents btnSelectNone As ToolStripButton
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents ToolStripLabel1 As ToolStripLabel
    Friend WithEvents txtExportTo As ToolStripTextBox
    Friend WithEvents btnBrowseExport As ToolStripButton
    Friend WithEvents btnExport As ToolStripButton
    Friend WithEvents btnSelectUnity As ToolStripButton
    Friend WithEvents lblUnityFolder As ToolStripLabel
    Friend WithEvents tvwFiles As TreeView
    Friend WithEvents imlIcons As ImageList

End Class
