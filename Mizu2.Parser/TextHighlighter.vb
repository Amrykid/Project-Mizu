' Generated by TinyPG v1.3 available at www.codeproject.com

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Threading

Namespace Mizu.Parser
    ' Summary:
    '     System.EventArgs is the base class for classes containing event data.
    <Serializable()> _
    <ComVisible(True)> _
    Public Class ContextSwitchEventArgs
        Inherits EventArgs
        Public ReadOnly PreviousContext As ParseNode
        Public ReadOnly NewContext As ParseNode

        ' Summary:
        '     Initializes a new instance of the System.EventArgs class.
        Public Sub New(ByVal prevContext As ParseNode, ByVal nextContext As ParseNode)
            PreviousContext = prevContext
            NewContext = nextContext
        End Sub
    End Class

    ' delegate for firing context switch events
    Public Delegate Sub ContextSwitchEventHandler(ByVal sender As Object, ByVal e As ContextSwitchEventArgs)

    ''' <summary>
    ''' Takes control over the RichTextBox and will color the text accoording to the rules of the parser and the scanner
    ''' this control extender will also support Undo/Redo functionality.
    ''' </summary>
    Public Class TextHighlighter
        Implements IDisposable
        Private Class UndoItem
            ''' <summary>
            ''' contains the information for an undo/redo action
            ''' </summary>
            ''' <param name="text">the full text to be undone/redone</param>
            ''' <param name="position">position of the caret after the un/redo action</param>
            ''' <param name="scroll">position of the scrollbars after un/redo action</param>
            Public Sub New(ByVal text As String, ByVal position As Integer, ByVal scroll As Point)
                Me.Text = text
                Me.Position = position
                Me.ScrollPosition = scroll
            End Sub

            Public Text As String
            Public Position As Integer
            Public ScrollPosition As Point
        End Class


        ' some winapís required
        <DllImport("user32", CharSet:=CharSet.Auto)> _
        Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Integer, ByVal wParam As Integer, ByVal lParam As IntPtr) As IntPtr
        End Function

        <DllImport("user32.dll")> _
        Private Shared Function PostMessageA(ByVal hWnd As IntPtr, ByVal nBar As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Boolean
        End Function

        <DllImport("user32.dll", CharSet:=CharSet.Auto)> _
        Private Shared Function GetScrollPos(ByVal hWnd As Integer, ByVal nBar As Integer) As Integer
        End Function

        <DllImport("user32.dll")> _
        Private Shared Function SetScrollPos(ByVal hWnd As IntPtr, ByVal nBar As Integer, ByVal nPos As Integer, ByVal bRedraw As Boolean) As Integer
        End Function

        Private Const WM_SETREDRAW As Integer = 11
        Private Const WM_USER As Integer = 1024
        Private Const EM_GETEVENTMASK As Integer = (WM_USER + 59)
        Private Const EM_SETEVENTMASK As Integer = (WM_USER + 69)
        Private Const SB_HORZ As Integer = 0
        Private Const SB_VERT As Integer = 1
        Private Const WM_HSCROLL As Integer = 276
        Private Const WM_VSCROLL As Integer = 277
        Private Const SB_THUMBPOSITION As Integer = 4
        Private Const UNDO_BUFFER As Integer = 100

        Private Property HScrollPos() As Integer
            Get
                Return GetScrollPos(Textbox.Handle.ToInt32(), SB_HORZ)
            End Get
            Set(ByVal value As Integer)
                SetScrollPos(DirectCast(Textbox.Handle, IntPtr), SB_HORZ, value, True)
                PostMessageA(DirectCast(Textbox.Handle, IntPtr), WM_HSCROLL, SB_THUMBPOSITION + 65536 * value, 0)
            End Set
        End Property

        Private Property VScrollPos() As Integer
            Get
                Return GetScrollPos(Textbox.Handle.ToInt32(), SB_VERT)
            End Get
            Set(ByVal value As Integer)
                SetScrollPos(DirectCast(Textbox.Handle, IntPtr), SB_VERT, value, True)
                PostMessageA(DirectCast(Textbox.Handle, IntPtr), WM_VSCROLL, SB_THUMBPOSITION + 65536 * value, 0)
            End Set
        End Property

        ' public shared members
        Public Tree As ParseTree
        Public ReadOnly Textbox As RichTextBox

        ' private members
        Private Parser As Parser
        Private Scanner As Scanner
        Private stateLocked As IntPtr = IntPtr.Zero

        Private UndoIndex As Integer = -1
        Private UndoList As List(Of UndoItem)

        Private currentContext As ParseNode
        Public Event SwitchContext As ContextSwitchEventHandler

        Private threadAutoHighlight As Thread


        Private Sub DoAction(ByVal text As String, ByVal position As Integer)

            If stateLocked <> IntPtr.Zero Then
                Return
            End If

            Dim ua As New UndoItem(text, position, New Point(HScrollPos, VScrollPos))
            UndoList.RemoveRange(UndoIndex, UndoList.Count - UndoIndex)
            UndoList.Add(ua)
            If UndoList.Count > UNDO_BUFFER Then
                UndoList.RemoveAt(0)
            End If

            ' make undo/redo a little smarter, remove single strokes
            ' reducing nr of undo states
            If UndoList.Count > 7 Then
                Dim canRemove As Boolean = True
                Dim nextItem As UndoItem = ua
                Dim i As Integer = 0
                While i < 6
    Dim prevItem As UndoItem = UndoList(UndoList.Count - 2 - i)
                    canRemove = canRemove And (Math.Abs(prevItem.Text.Length - nextItem.Text.Length) <= 1 AndAlso Math.Abs(prevItem.Position - nextItem.Position) <= 1)
                    nextItem = prevItem
                    System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
                End While
                If canRemove Then
                    UndoList.RemoveRange(UndoList.Count - 6, 5)
                End If
            End If
            UndoIndex = UndoList.Count
        End Sub

    Public Sub ClearUndo()
        UndoList = New List(Of UndoItem)()
        UndoIndex = 0
    End Sub

    Public Sub Undo()
        If Not CanUndo Then
            Return
        End If

        System.Math.Max(System.Threading.Interlocked.Decrement(UndoIndex), UndoIndex + 1)
        If UndoIndex < 1 Then
            UndoIndex = 1
        End If

        ' implement undo action here
        Dim ua As UndoItem = UndoList(UndoIndex - 1)
        RestoreState(ua)
    End Sub

    Public Sub Redo()
        If Not CanRedo Then
            Return
        End If

        System.Math.Max(System.Threading.Interlocked.Increment(UndoIndex), UndoIndex - 1)
        If UndoIndex > UndoList.Count Then
            UndoIndex = UndoList.Count
        End If

        Dim ua As UndoItem = UndoList(UndoIndex - 1)
        RestoreState(ua)

    End Sub

    Private Sub RestoreState(ByVal item As UndoItem)
        Lock()
        ' restore state
        Textbox.Rtf = item.Text
        Textbox.[Select](item.Position, 0)
        HScrollPos = item.ScrollPosition.X
        VScrollPos = item.ScrollPosition.Y

        Unlock()
    End Sub

    Public ReadOnly Property CanUndo() As Boolean
        Get
            Return UndoIndex > 0
        End Get
    End Property

    Public ReadOnly Property CanRedo() As Boolean
        Get
            Return UndoIndex < UndoList.Count
        End Get
    End Property

    Public Sub New(ByVal textbox As RichTextBox, ByVal scanner As Scanner, ByVal parser As Parser)
        Me.Textbox = textbox
        Me.Scanner = scanner
        Me.Parser = parser

        ClearUndo()

        AddHandler Textbox.TextChanged, AddressOf Textbox_TextChanged
        AddHandler textbox.KeyDown, AddressOf textbox_KeyDown
        AddHandler Textbox.SelectionChanged, AddressOf Textbox_SelectionChanged
        AddHandler Textbox.Disposed, AddressOf Textbox_Disposed

        Tree = New ParseTree()
        currentContext = Tree

        threadAutoHighlight = New Thread(AddressOf AutoHighlightStart)
        threadAutoHighlight.Start()
    End Sub


    Public Sub Lock()
        ' Stop redrawing:  
        SendMessage(Textbox.Handle, WM_SETREDRAW, 0, IntPtr.Zero)
        ' Stop sending of events:  
        stateLocked = SendMessage(Textbox.Handle, EM_GETEVENTMASK, 0, IntPtr.Zero)
        ' change colors and stuff in the RichTextBox  
    End Sub

    Public Sub Unlock()
        ' turn on events  
        SendMessage(Textbox.Handle, EM_SETEVENTMASK, 0, stateLocked)
        ' turn on redrawing  
        SendMessage(Textbox.Handle, WM_SETREDRAW, 1, IntPtr.Zero)

        stateLocked = IntPtr.Zero
        Textbox.Invalidate()
    End Sub

    Sub textbox_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        ' undo/redo
        If e.KeyValue = 89 AndAlso e.Control Then
            Redo()
            ' CTRL-Y
        End If
        If e.KeyValue = 90 AndAlso e.Control Then
            Undo()
            ' CTRL-Z
        End If
    End Sub

    Sub Textbox_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        If stateLocked <> IntPtr.Zero Then
            Return
        End If

        DoAction(Textbox.Rtf, Textbox.SelectionStart)

        HighlightText()
    End Sub

    Sub Textbox_SelectionChanged(ByVal sender As Object, ByVal e As EventArgs)
        If stateLocked <> IntPtr.Zero Then
            Return
        End If

        Dim newContext As ParseNode = GetCurrentContext()

        If currentContext Is Nothing Then
            currentContext = newContext
        End If
        If newContext Is Nothing Then
            Return
        End If

        If newContext.Token.Type <> currentContext.Token.Type Then
            RaiseEvent SwitchContext(Me, New ContextSwitchEventArgs(currentContext, newContext))
            'SwitchContext.Invoke(Me, New ContextSwitchEventArgs(currentContext, newContext))
            currentContext = newContext
        End If

    End Sub

    ''' <summary>
    ''' this handy function returns the section in which the user is editing currently
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCurrentContext() As ParseNode
        Dim node As ParseNode = FindNode(Tree, Textbox.SelectionStart)
        Return node
    End Function

    Private Function FindNode(ByVal node As ParseNode, ByVal posstart As Integer) As ParseNode

        If node Is Nothing Then
            Return Nothing
        End If

        If node.Token.StartPos <= posstart AndAlso (node.Token.StartPos + node.Token.Length) >= posstart Then
            For Each n As ParseNode In node.Nodes
                If n.Token.StartPos <= posstart AndAlso (n.Token.StartPos + n.Token.Length) >= posstart Then
                    Return FindNode(n, posstart)
                End If
            Next
            Return node
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' use HighlighText to start the text highlight process from the caller's thread.
    ''' this method is not used internally. 
    ''' </summary>
    Public Sub HighlightText()
        SyncLock treelock
            textChanged = True
            currentText = Textbox.Text
        End SyncLock
    End Sub

    Private Sub HighlightTextInternal()
        ' highlight the text (used internally only)
        Lock()

        Dim hscroll As Integer = HScrollPos
        Dim vscroll As Integer = VScrollPos

        Dim selstart As Integer = Textbox.SelectionStart

        HighlighTextCore()

        Textbox.[Select](selstart, 0)

        HScrollPos = hscroll
        VScrollPos = vscroll

        Unlock()
    End Sub

    ''' <summary>
    ''' this method should be used only by HighlightText or RestoreState methods
    ''' </summary>
    Private Sub HighlighTextCore()
        'Tree = Parser.Parse(Textbox.Text);
        Dim sb As New StringBuilder()
        If Tree Is Nothing Then
            Return
        End If

        Dim start As ParseNode = Tree.Nodes(0)
        HightlightNode(start, sb)

        ' append any trailing skipped tokens that were scanned
        For Each skiptoken As Token In Scanner.Skipped
            HighlightToken(skiptoken, sb)
            sb.Append(skiptoken.Text.Replace("\", "\\").Replace("{", "\{").Replace("}", "\}").Replace(vbLf, "\par" & vbLf))
        Next

        sb = Unicode(sb)     ' <--- without this, unicode characters will be garbled after highlighting

        AddRtfHeader(sb)
        AddRtfEnd(sb)

        Textbox.Rtf = sb.ToString()

    End Sub


    ''' <summary>
    ''' added function to convert unicode characters in the stringbuilder to rtf unicode escapes
    ''' </summary>
    Function Unicode(ByVal sb As StringBuilder) As StringBuilder

        Dim i As Integer
        Dim uc As StringBuilder = New StringBuilder
        For i = 0 To sb.Length - 1
            Dim c As Char = sb(i)
            If AscW(c) < 127 Then
                uc.Append(c.ToString())
            Else
                uc.Append("\u" & CStr(AscW(c)) + "?")
            End If
        Next

        Return uc

    End Function

    ' thread start for the automatic highlighting
    Private Shared treelock As New Object()
    Private isDisposing As Boolean
    Private textChanged As Boolean
    Private currentText As String

    Private Sub AutoHighlightStart()
        Dim _tree As ParseTree
        Dim _currenttext As String = ""
        While Not isDisposing
            Dim _textchanged As Boolean
            SyncLock treelock
                _textchanged = textChanged
                If textChanged Then
                    textChanged = False
                    _currenttext = currentText
                End If
            End SyncLock
            If Not _textchanged Then
                Thread.Sleep(200)
                Continue While
            End If

            _tree = DirectCast(Parser.Parse(_currenttext), ParseTree)

            SyncLock treelock
                If textChanged Then
                    Continue While
                Else
                    ' assign new tree
                    Tree = _tree
                End If
            End SyncLock


            Textbox.Invoke(New MethodInvoker(AddressOf HighlightTextInternal))
        End While
    End Sub


    ''' <summary>
    ''' inserts the RTF codes to highlight text blocks
    ''' </summary>
    ''' <param name="node">the node to highlight, will be appended to sb</param>
    ''' <param name="sb">the final output string</param>
    Private Sub HightlightNode(ByVal node As ParseNode, ByVal sb As StringBuilder)
        If node.Nodes.Count = 0 Then
            If (node.Token.Skipped IsNot Nothing) Then
                For Each skiptoken As Token In node.Token.Skipped
                    HighlightToken(skiptoken, sb)
                    sb.Append(skiptoken.Text.Replace("\", "\\").Replace("{", "\{").Replace("}", "\}").Replace(vbLf, "\par" & vbLf))
                Next
            End If

            HighlightToken(node.Token, sb)
            sb.Append(node.Token.Text.Replace("\", "\\").Replace("{", "\{").Replace("}", "\}").Replace("" & Chr(10) & "", "\par" & Chr(10) & ""))
            sb.Append("}")
        End If

        For Each n As ParseNode In node.Nodes
            HightlightNode(n, sb)
        Next
    End Sub

    ''' <summary>
    ''' inserts the RTF codes to highlight text blocks
    ''' </summary>
    ''' <param name="token">the token to highlight, will be appended to sb</param>
    ''' <param name="sb">the final output string</param>
    Private Sub HighlightToken(ByVal token As Token, ByVal sb As StringBuilder)
        Select Case token.Type
                    Case TokenType.STRING:
                        sb.Append("{{\cf1 ")
                        Exit Select
                    Case TokenType.IDENTIFIER:
                        sb.Append("{{\cf2 ")
                        Exit Select
                    Case TokenType.NUMBER:
                        sb.Append("{{\cf3 ")
                        Exit Select
                    Case TokenType.FLOAT:
                        sb.Append("{{\cf4 ")
                        Exit Select
                    Case TokenType.BROPEN:
                        sb.Append("{{\cf5 ")
                        Exit Select
                    Case TokenType.BRCLOSE:
                        sb.Append("{{\cf6 ")
                        Exit Select

            Case Else
                sb.Append("{{\cf0 ")
                Exit Select
        End Select
    End Sub

    ' define the color palette to be used here
    Private Sub AddRtfHeader(ByVal sb As StringBuilder)
        sb.Insert(0, "{\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Consolas;}}{\colortbl;\red255\green0\blue0;\red0\green0\blue255;\red255\green0\blue0;\red255\green0\blue0;\red0\green0\blue255;\red0\green0\blue255;}\viewkind4\uc1\pard\lang1033\f0\fs20")
    End Sub

    Private Sub AddRtfEnd(ByVal sb As StringBuilder)
        sb.Append("}")
    End Sub

    Sub Textbox_Disposed(ByVal sender As Object, ByVal e As EventArgs)
        Dispose()
    End Sub

#Region "IDisposable Members"

    Public Sub Dispose() Implements IDisposable.Dispose
        isDisposing = True
        threadAutoHighlight.Join(1000)
        If threadAutoHighlight.IsAlive Then
            threadAutoHighlight.Abort()
        End If
    End Sub

#End Region

    End Class
End Namespace
