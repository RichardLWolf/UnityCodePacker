Imports System.Collections
Imports System.Windows.Forms

Public Class ListviewSorter
    Implements System.Collections.IComparer

    Private m_ColumnNumber As Integer
    Private m_SortOrder As SortOrder
    Private m_ForceStringCompare As Boolean

    Public Sub New(ByVal column_number As Integer, ByVal _
        sort_order As SortOrder, Optional ByVal [ForceStringCompare] As Boolean = False)
        m_ColumnNumber = column_number
        m_SortOrder = sort_order
        m_ForceStringCompare = [ForceStringCompare]
    End Sub

    ' Compare the items in the appropriate column
    ' for objects x and y.
    Public Function Compare(ByVal x As Object, ByVal y As _
        Object) As Integer Implements _
        System.Collections.IComparer.Compare
        Dim item_x As ListViewItem = DirectCast(x, _
            ListViewItem)
        Dim item_y As ListViewItem = DirectCast(y, _
            ListViewItem)

        ' Get the sub-item values.
        Dim string_x As String
        If item_x.SubItems.Count <= m_ColumnNumber Then
            string_x = ""
        Else
            string_x = item_x.SubItems(m_ColumnNumber).Text
        End If

        Dim string_y As String
        If item_y.SubItems.Count <= m_ColumnNumber Then
            string_y = ""
        Else
            string_y = item_y.SubItems(m_ColumnNumber).Text
        End If

        ' Compare them.
        If m_ForceStringCompare Then
            If m_SortOrder = SortOrder.Ascending Then
                Return String.Compare(string_x, string_y)
            Else
                Return String.Compare(string_y, string_x)
            End If
        Else
            If m_SortOrder = SortOrder.Ascending Then
                If IsNumeric(string_x) And IsNumeric(string_y) _
                    Then
                    Return Val(string_x).CompareTo(Val(string_y))
                ElseIf IsDate(string_x) And IsDate(string_y) _
                    Then
                    Return DateTime.Parse(string_x).CompareTo(DateTime.Parse(string_y))
                Else
                    Return String.Compare(string_x, string_y)
                End If
            Else
                If IsNumeric(string_x) And IsNumeric(string_y) _
                    Then
                    Return Val(string_y).CompareTo(Val(string_x))
                ElseIf IsDate(string_x) And IsDate(string_y) _
                    Then
                    Return DateTime.Parse(string_y).CompareTo(DateTime.Parse(string_x))
                Else
                    Return String.Compare(string_y, string_x)
                End If
            End If
        End If
    End Function

    Public Property SortColumn() As Integer
        Get
            Return m_ColumnNumber
        End Get
        Set(ByVal Value As Integer)
            m_ColumnNumber = Value
        End Set
    End Property

    Public Property Order() As SortOrder
        Get
            Return m_SortOrder
        End Get
        Set(ByVal Value As SortOrder)
            m_SortOrder = Value
        End Set
    End Property

    Public Property ForceStringCompare() As Boolean
        Get
            ForceStringCompare = m_ForceStringCompare
        End Get
        Set(ByVal value As Boolean)
            m_ForceStringCompare = value
        End Set
    End Property
End Class

