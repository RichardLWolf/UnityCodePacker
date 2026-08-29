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
        btnRefresh = New ToolStripButton()
        btnSelectAll = New ToolStripButton()
        btnSelectNone = New ToolStripButton()
        btnExclude = New ToolStripButton()
        btnIncludeMetaFiles = New ToolStripButton()
        ToolStripSeparator1 = New ToolStripSeparator()
        ToolStripLabel1 = New ToolStripLabel()
        txtExportTo = New ToolStripTextBox()
        btnBrowseExport = New ToolStripButton()
        btnSelectUnity = New ToolStripButton()
        lblUnityFolder = New ToolStripLabel()
        cboOutput = New ToolStripComboBox()
        btnExport = New ToolStripButton()
        ToolStripSeparator2 = New ToolStripSeparator()
        tvwFiles = New TreeView()
        imlIcons = New ImageList(components)
        splBase = New SplitContainer()
        panSearch = New Panel()
        txtWorkbenchFolder = New TextBox()
        Label1 = New Label()
        btnSetWorkbench = New Button()
        btnCollapseAll = New Button()
        btxExpandAll = New Button()
        btnClearText = New Button()
        btnSearch = New Button()
        txtSearch = New TextBox()
        lvwSelected = New ListView()
        ToolTip1 = New ToolTip(components)
        tspMenu.SuspendLayout()
        CType(splBase, ComponentModel.ISupportInitialize).BeginInit()
        splBase.Panel1.SuspendLayout()
        splBase.Panel2.SuspendLayout()
        splBase.SuspendLayout()
        panSearch.SuspendLayout()
        SuspendLayout()
        ' 
        ' tspMenu
        ' 
        tspMenu.BackColor = SystemColors.ControlDark
        tspMenu.GripStyle = ToolStripGripStyle.Hidden
        tspMenu.ImageScalingSize = New Size(32, 32)
        tspMenu.Items.AddRange(New ToolStripItem() {btnRefresh, btnSelectAll, btnSelectNone, btnExclude, btnIncludeMetaFiles, ToolStripSeparator1, ToolStripLabel1, txtExportTo, btnBrowseExport, btnSelectUnity, lblUnityFolder, cboOutput, btnExport, ToolStripSeparator2})
        tspMenu.Location = New Point(0, 0)
        tspMenu.Name = "tspMenu"
        tspMenu.Padding = New Padding(10, 0, 10, 0)
        tspMenu.Size = New Size(1008, 39)
        tspMenu.TabIndex = 0
        tspMenu.Text = "ToolStrip1"
        ' 
        ' btnRefresh
        ' 
        btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Image
        btnRefresh.Image = CType(resources.GetObject("btnRefresh.Image"), Image)
        btnRefresh.ImageTransparentColor = Color.Magenta
        btnRefresh.Name = "btnRefresh"
        btnRefresh.Size = New Size(36, 36)
        btnRefresh.Text = "Refresh"
        btnRefresh.TextImageRelation = TextImageRelation.ImageAboveText
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
        ' btnExclude
        ' 
        btnExclude.DisplayStyle = ToolStripItemDisplayStyle.Image
        btnExclude.Image = CType(resources.GetObject("btnExclude.Image"), Image)
        btnExclude.ImageTransparentColor = Color.Magenta
        btnExclude.Margin = New Padding(0, 1, 5, 2)
        btnExclude.Name = "btnExclude"
        btnExclude.Size = New Size(36, 36)
        btnExclude.Text = "Exclude Folders"
        btnExclude.TextAlign = ContentAlignment.BottomCenter
        btnExclude.ToolTipText = "Exclude selected folders"
        ' 
        ' btnIncludeMetaFiles
        ' 
        btnIncludeMetaFiles.CheckOnClick = True
        btnIncludeMetaFiles.DisplayStyle = ToolStripItemDisplayStyle.Image
        btnIncludeMetaFiles.Image = CType(resources.GetObject("btnIncludeMetaFiles.Image"), Image)
        btnIncludeMetaFiles.ImageTransparentColor = Color.Magenta
        btnIncludeMetaFiles.Name = "btnIncludeMetaFiles"
        btnIncludeMetaFiles.Size = New Size(36, 36)
        btnIncludeMetaFiles.Text = "Include META"
        btnIncludeMetaFiles.ToolTipText = "Include META files"
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
        btnBrowseExport.ImageAlign = ContentAlignment.MiddleLeft
        btnBrowseExport.ImageTransparentColor = Color.Magenta
        btnBrowseExport.Name = "btnBrowseExport"
        btnBrowseExport.Padding = New Padding(0, 0, 10, 0)
        btnBrowseExport.Size = New Size(46, 36)
        btnBrowseExport.Text = "Browse folder"
        btnBrowseExport.TextImageRelation = TextImageRelation.ImageAboveText
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
        lblUnityFolder.AutoSize = False
        lblUnityFolder.ForeColor = Color.Blue
        lblUnityFolder.Name = "lblUnityFolder"
        lblUnityFolder.Overflow = ToolStripItemOverflow.Never
        lblUnityFolder.Size = New Size(250, 36)
        lblUnityFolder.Text = "Select Unity Folder"
        lblUnityFolder.TextAlign = ContentAlignment.MiddleRight
        lblUnityFolder.ToolTipText = "Selected Unity folder root."
        ' 
        ' cboOutput
        ' 
        cboOutput.Items.AddRange(New Object() {"ZIP Only", "Zip and JSON Codepack"})
        cboOutput.Name = "cboOutput"
        cboOutput.Size = New Size(121, 39)
        cboOutput.ToolTipText = "Output Options"
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
        ' ToolStripSeparator2
        ' 
        ToolStripSeparator2.Margin = New Padding(0, 0, 5, 0)
        ToolStripSeparator2.Name = "ToolStripSeparator2"
        ToolStripSeparator2.Size = New Size(6, 39)
        ' 
        ' tvwFiles
        ' 
        tvwFiles.CheckBoxes = True
        tvwFiles.Dock = DockStyle.Fill
        tvwFiles.Font = New Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        tvwFiles.ImageIndex = 0
        tvwFiles.ImageList = imlIcons
        tvwFiles.Location = New Point(0, 77)
        tvwFiles.Name = "tvwFiles"
        tvwFiles.SelectedImageIndex = 0
        tvwFiles.Size = New Size(658, 334)
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
        imlIcons.Images.SetKeyName(3, "meta_file")
        ' 
        ' splBase
        ' 
        splBase.Dock = DockStyle.Fill
        splBase.Location = New Point(0, 39)
        splBase.Name = "splBase"
        ' 
        ' splBase.Panel1
        ' 
        splBase.Panel1.Controls.Add(tvwFiles)
        splBase.Panel1.Controls.Add(panSearch)
        ' 
        ' splBase.Panel2
        ' 
        splBase.Panel2.Controls.Add(lvwSelected)
        splBase.Size = New Size(1008, 411)
        splBase.SplitterDistance = 658
        splBase.TabIndex = 2
        ' 
        ' panSearch
        ' 
        panSearch.BackColor = SystemColors.ControlDark
        panSearch.Controls.Add(txtWorkbenchFolder)
        panSearch.Controls.Add(Label1)
        panSearch.Controls.Add(btnSetWorkbench)
        panSearch.Controls.Add(btnCollapseAll)
        panSearch.Controls.Add(btxExpandAll)
        panSearch.Controls.Add(btnClearText)
        panSearch.Controls.Add(btnSearch)
        panSearch.Controls.Add(txtSearch)
        panSearch.Dock = DockStyle.Top
        panSearch.Location = New Point(0, 0)
        panSearch.Name = "panSearch"
        panSearch.Size = New Size(658, 77)
        panSearch.TabIndex = 2
        ' 
        ' txtWorkbenchFolder
        ' 
        txtWorkbenchFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtWorkbenchFolder.Font = New Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtWorkbenchFolder.Location = New Point(113, 6)
        txtWorkbenchFolder.Name = "txtWorkbenchFolder"
        txtWorkbenchFolder.Size = New Size(504, 29)
        txtWorkbenchFolder.TabIndex = 9
        txtWorkbenchFolder.Text = "N:\Repository\CoWGameWorkbench"
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(3, 14)
        Label1.Name = "Label1"
        Label1.Size = New Size(104, 15)
        Label1.TabIndex = 8
        Label1.Text = "Workbnech Folder"
        ' 
        ' btnSetWorkbench
        ' 
        btnSetWorkbench.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSetWorkbench.FlatStyle = FlatStyle.Popup
        btnSetWorkbench.Image = CType(resources.GetObject("btnSetWorkbench.Image"), Image)
        btnSetWorkbench.Location = New Point(623, 6)
        btnSetWorkbench.Name = "btnSetWorkbench"
        btnSetWorkbench.Size = New Size(30, 30)
        btnSetWorkbench.TabIndex = 7
        ToolTip1.SetToolTip(btnSetWorkbench, "Set CoW Workbench project folder")
        btnSetWorkbench.UseVisualStyleBackColor = True
        ' 
        ' btnCollapseAll
        ' 
        btnCollapseAll.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btnCollapseAll.FlatStyle = FlatStyle.Popup
        btnCollapseAll.Image = CType(resources.GetObject("btnCollapseAll.Image"), Image)
        btnCollapseAll.Location = New Point(39, 41)
        btnCollapseAll.Name = "btnCollapseAll"
        btnCollapseAll.Size = New Size(30, 30)
        btnCollapseAll.TabIndex = 6
        ToolTip1.SetToolTip(btnCollapseAll, "Collapse all nodes")
        btnCollapseAll.UseVisualStyleBackColor = True
        ' 
        ' btxExpandAll
        ' 
        btxExpandAll.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left
        btxExpandAll.FlatStyle = FlatStyle.Popup
        btxExpandAll.Image = CType(resources.GetObject("btxExpandAll.Image"), Image)
        btxExpandAll.Location = New Point(3, 41)
        btxExpandAll.Name = "btxExpandAll"
        btxExpandAll.Size = New Size(30, 30)
        btxExpandAll.TabIndex = 5
        ToolTip1.SetToolTip(btxExpandAll, "Expand all nodes")
        btxExpandAll.UseVisualStyleBackColor = True
        ' 
        ' btnClearText
        ' 
        btnClearText.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnClearText.FlatStyle = FlatStyle.Popup
        btnClearText.Image = CType(resources.GetObject("btnClearText.Image"), Image)
        btnClearText.Location = New Point(587, 42)
        btnClearText.Name = "btnClearText"
        btnClearText.Size = New Size(30, 30)
        btnClearText.TabIndex = 4
        ToolTip1.SetToolTip(btnClearText, "Clear search text")
        btnClearText.UseVisualStyleBackColor = True
        ' 
        ' btnSearch
        ' 
        btnSearch.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnSearch.FlatStyle = FlatStyle.Popup
        btnSearch.Image = CType(resources.GetObject("btnSearch.Image"), Image)
        btnSearch.Location = New Point(623, 41)
        btnSearch.Name = "btnSearch"
        btnSearch.Size = New Size(30, 30)
        btnSearch.TabIndex = 3
        ToolTip1.SetToolTip(btnSearch, "Find next serach text")
        btnSearch.UseVisualStyleBackColor = True
        ' 
        ' txtSearch
        ' 
        txtSearch.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        txtSearch.Font = New Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        txtSearch.Location = New Point(75, 42)
        txtSearch.Name = "txtSearch"
        txtSearch.Size = New Size(506, 29)
        txtSearch.TabIndex = 0
        ToolTip1.SetToolTip(txtSearch, "Search text")
        ' 
        ' lvwSelected
        ' 
        lvwSelected.BackColor = Color.Azure
        lvwSelected.Dock = DockStyle.Fill
        lvwSelected.Font = New Font("Segoe UI", 12F)
        lvwSelected.ForeColor = Color.Black
        lvwSelected.Location = New Point(0, 0)
        lvwSelected.Name = "lvwSelected"
        lvwSelected.Size = New Size(346, 411)
        lvwSelected.TabIndex = 0
        lvwSelected.UseCompatibleStateImageBehavior = False
        ' 
        ' frmMain
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1008, 450)
        Controls.Add(splBase)
        Controls.Add(tspMenu)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "frmMain"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Unity Code Packer"
        tspMenu.ResumeLayout(False)
        tspMenu.PerformLayout()
        splBase.Panel1.ResumeLayout(False)
        splBase.Panel2.ResumeLayout(False)
        CType(splBase, ComponentModel.ISupportInitialize).EndInit()
        splBase.ResumeLayout(False)
        panSearch.ResumeLayout(False)
        panSearch.PerformLayout()
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
    Friend WithEvents btnRefresh As ToolStripButton
    Friend WithEvents btnSelectUnity As ToolStripButton
    Friend WithEvents lblUnityFolder As ToolStripLabel
    Friend WithEvents tvwFiles As TreeView
    Friend WithEvents imlIcons As ImageList
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents splBase As SplitContainer
    Friend WithEvents panSearch As Panel
    Friend WithEvents btnSearch As Button
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents btnExport As ToolStripButton
    Friend WithEvents lvwSelected As ListView
    Friend WithEvents cboOutput As ToolStripComboBox
    Friend WithEvents btnExclude As ToolStripButton
    Friend WithEvents btnIncludeMetaFiles As ToolStripButton
    Friend WithEvents btnClearText As Button
    Friend WithEvents btxExpandAll As Button
    Friend WithEvents btnCollapseAll As Button
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents Label1 As Label
    Friend WithEvents btnSetWorkbench As Button
    Friend WithEvents txtWorkbenchFolder As TextBox

End Class
