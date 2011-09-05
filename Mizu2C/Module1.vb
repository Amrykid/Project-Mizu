﻿Imports Mizu2.Parser
Imports System.Dynamic
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.IO

Module Module1

    Sub Main(ByVal args As String())
        If args.Count >= 2 Then
            If IO.File.Exists(args(0)) = True Then
                Dim scanner As New Scanner
                Dim parser As New Parser(scanner)

                Code = IO.File.ReadAllText(args(0))

                If code.Length = 0 Then
                    Console.Error.WriteLine("Source code file cannot be empty.")
                    Return
                End If

                Dim tree = parser.Parse(code)

                If tree.Errors.Count > 0 Then
                    For Each Err As ParseError In tree.Errors
                        Console.Error.WriteLine("[{0},{1}] Error: {2}", Err.Line + 1, Err.Position, Err.Message)
                    Next
                    Return
                Else
                    Dim input As New FileInfo(args(0)), output As New FileInfo(args(1))
                    Compile(input, output, tree)
                End If
            Else
                Console.Error.WriteLine("File doesn't exist!")
                Return
            End If
        Else
            Console.Error.WriteLine("Not enough parameters.")
            Return
        End If
    End Sub
    Public IsDebug As Boolean = False
    Public Code As String = Nothing
    Public Doc As System.Diagnostics.SymbolStore.ISymbolDocumentWriter
    Public Function Compile(ByVal input As FileInfo, output As FileInfo, ByVal tree As ParseTree) As Boolean
        'The majority of this was ported from the first Mizu (Mizu Concept 1).

        Dim statements = tree.Nodes(0)

        'Declares the assembly and the entypoint
        Dim name = New AssemblyName(output.Name.Replace(output.Extension, ""))
        Dim ab As AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Save, output.DirectoryName)

        If (IsDebug) Then

            'Make assembly debug-able.
            Dim debugattr = GetType(DebuggableAttribute)
            Dim db_const = debugattr.GetConstructor(New Type() {GetType(DebuggableAttribute.DebuggingModes)})
            Dim db_builder = New CustomAttributeBuilder(db_const, New Object() {DebuggableAttribute.DebuggingModes.DisableOptimizations Or
            DebuggableAttribute.DebuggingModes.Default})

            ab.SetCustomAttribute(db_builder)
        End If

        ' Defines main module.
        Dim mb = ab.DefineDynamicModule(name.Name, name.Name + ".exe", IsDebug)

        If (IsDebug) Then
            'Define the source code file.
            Doc = mb.DefineDocument(input.FullName, Guid.Empty, Guid.Empty, Guid.Empty)
        End If

        Dim tb = mb.DefineType("App") 'Defines main type.
        Dim entrypoint = tb.DefineMethod("Main", MethodAttributes.Public + MethodAttributes.Static) 'Makes the main method.

        Dim ILgen = entrypoint.GetILGenerator(3072) 'gets the IL generator

        Dim locals As New List(Of LocalBuilderEx) 'A list to hold variables.

        ILgen.BeginExceptionBlock() 'Start a try statement.

        ILgen.BeginScope()

        ' Generate body IL
        Dim err = False
        For Each statement As ParseNode In statements.Nodes
            'Iterate though the statements, generating IL.
            Dim basestmt = statement.Nodes(0)
            HandleStatement(basestmt, ILgen, locals, err)
            If (err = True) Then
                Return False
            End If
        Next

        ILgen.EndScope()

        ILgen.BeginCatchBlock(GetType(Exception))

        ILgen.Emit(OpCodes.Callvirt, GetType(Exception).GetMethod("ToString"))

        ILgen.Emit(OpCodes.Call, GetType(Console).GetMethod("WriteLine", New Type() {GetType(String)}))

        ILgen.EndExceptionBlock()  'Ends the catch section.


        ILgen.Emit(OpCodes.Ret) 'Finishes the statement by calling return.

        ab.SetEntryPoint(entrypoint, PEFileKinds.ConsoleApplication) 'Sets entry point

        Dim finishedtype = tb.CreateType() 'Compile the type

        ab.Save(output.Name) 'Save
        Return True
    End Function
    Public Sub HandleExprAsAssignment(ByVal expr As ParseNode, ByVal ILgen As ILGenerator, ByVal locals As List(Of LocalBuilderEx), ByRef err As Boolean)

    End Sub
    Public Sub HandleExprAsBoolean(ByVal expr As ParseNode, ByVal ILgen As ILGenerator, ByVal locals As List(Of LocalBuilderEx), ByRef err As Boolean)
        Dim left As ParseNode = expr.Nodes(0)
        Dim middle As ParseNode = expr.Nodes(1) 'The operator
        If middle.Token.Type = TokenType.WHITESPACE Then middle = expr.Nodes(2)

        Dim right As ParseNode = expr.Nodes(3)
        If right.Token.Type = TokenType.WHITESPACE Then right = expr.Nodes(4)

        LoadToken(ILgen, left, locals, err)
        If err Then Return

        LoadToken(ILgen, right, locals, err)
        If err Then Return

        LoadOperator(middle, ILgen)
    End Sub
    Public Function HandleFunctionCall(ByVal stmt As ParseNode, ByVal ILgen As ILGenerator, ByRef locals As List(Of LocalBuilderEx), ByRef err As Boolean) As Boolean
        Dim returnsvalues As Boolean = False

        Dim params As ParseNode() = Nothing
        Dim usedident As Boolean = False
        Dim ident As LocalBuilderEx = Nothing
        Dim func = TypeResolver.ResolveFunctionFromParseNode(stmt, locals, params, usedident, ident)

        If usedident = False Then

            returnsvalues = func.ReturnType <> GetType(Void)

            For Each param In params 'If any parameters, load them.
                LoadToken(ILgen, param, locals, err)

                If err Then Return False
            Next

            ILgen.Emit(OpCodes.Call, func)

            Return returnsvalues
        Else
            returnsvalues = func.ReturnType <> GetType(Void)

            ILgen.Emit(OpCodes.Ldloca, ident.BaseLocal)
            'ILgen.Emit(OpCodes.Box, ident.VariableType)
            ILgen.Emit(OpCodes.Call, func)

            Return returnsvalues
        End If
    End Function
    Public Sub HandleStatement(ByVal stmt As ParseNode, ByVal ILgen As ILGenerator, ByRef locals As List(Of LocalBuilderEx), ByRef err As Boolean)
        Select Case stmt.Token.Type
            Case TokenType.FuncCall
                Dim returns = HandleFunctionCall(stmt, ILgen, locals, err)
                If returns = True Then
                    ILgen.Emit(OpCodes.Pop) 'Discard the value because in this context, we don't care about it.
                End If
                Return
            Case TokenType.IfStatement
                Dim expr As ParseNode = stmt.Nodes(2)
                Dim ifbody As ParseNode = stmt.Nodes.Find(Function(it) it.Token.Type = TokenType.IfStmtIFBody)
                Dim ifbodylabel As Label = ILgen.DefineLabel()
                Dim endofstmt As Label = ILgen.DefineLabel()

                'Handle the expression
                HandleExprAsBoolean(expr, ILgen, locals, err)
                If err Then Return

                ILgen.Emit(OpCodes.Brtrue, ifbodylabel)
                ILgen.Emit(OpCodes.Br, endofstmt) 'Otherwise, skip the method

                If (IsDebug) Then

                    Dim sline = 0, scol = 0

                    FindLineAndCol(Code, expr.Token.StartPos, sline, scol)

                    Dim eline = 0, ecol = 0

                    FindLineAndCol(Code, expr.Token.EndPos, eline, ecol)

                    ILgen.MarkSequencePoint(Doc, sline, scol, eline, ecol)
                End If

                ILgen.BeginScope()

                ILgen.MarkLabel(ifbodylabel)

                ''Handle inner statements here.

                Dim tmp_locals As New List(Of LocalBuilderEx) 'Create a temp list for local variables inside of the if statement.
                locals.ForEach(Sub(it) tmp_locals.Add(it)) 'Add global variables into the temp list.

                For Each ifstmt In ifbody.Nodes
                    HandleStatement(ifstmt.Nodes(0), ILgen, tmp_locals, err)

                    If err = True Then Return
                Next
                ILgen.Emit(OpCodes.Br, endofstmt)


                ILgen.MarkLabel(endofstmt)
                ILgen.EndScope()

                Return
            Case TokenType.VariableAssignment

                If (IsDebug) Then

                    Dim sline = 0, scol = 0

                    FindLineAndCol(Code, stmt.Token.StartPos, sline, scol)

                    Dim eline = 0, ecol = 0

                    FindLineAndCol(Code, stmt.Token.EndPos, eline, ecol)

                    ILgen.MarkSequencePoint(Doc, sline, scol, eline, ecol)
                End If


                Dim local As New LocalBuilderEx()

                Dim loc As ParseNode = stmt.Nodes(2)
                Dim name As String = loc.Nodes(0).Token.Text
                Dim value As ParseNode = loc.Nodes(loc.Nodes.Count - 1)

                Select Case loc.Nodes(2).Token.Type
                    Case TokenType.EQUAL
                        'var x = bla
                        'Use AS instead of = for declaring .NET objects

                        local.VariableName = name

                        LoadToken(ILgen, value, locals, err, local)
                        If err = True Then Return

                        If local.VariableType = Nothing Then
                            local.VariableType = GetType(Object) 'Just set it as object
                        End If
                        local.BaseLocal = ILgen.DeclareLocal(local.VariableType)

                        If (IsDebug) Then
                            local.BaseLocal.SetLocalSymInfo(name) 'Set variable name for debug info.
                        End If

                        ILgen.Emit(OpCodes.Stloc, local.BaseLocal)

                        locals.Add(local)
                        Return
                        Return
                    Case TokenType.AS


                        Dim typename As String = loc.Nodes(6).Token.Text

                        Dim constrs As ParseNode() = loc.Nodes.GetRange(8, loc.Nodes.Count - 8).ToArray()
                        constrs = Array.FindAll(constrs, Function(it) it.Token.Type <> TokenType.BROPEN And it.Token.Type <> TokenType.BRCLOSE)

                        Dim objType As Type = TypeResolver.ResolveType(typename)

                        local.VariableType = objType
                        local.BaseLocal = ILgen.DeclareLocal(local.VariableType)

                        If (IsDebug) Then
                            local.BaseLocal.SetLocalSymInfo(name) 'Set variable name for debug info.
                        End If

                        If constrs.Length = 0 Then
                            Dim constrInfo As ConstructorInfo = Nothing
                            constrInfo = objType.GetConstructor(New Type() {})


                            ILgen.Emit(OpCodes.Newobj, constrInfo)
                            ILgen.Emit(OpCodes.Stloc, local.BaseLocal)
                        ElseIf constrs.Length > 0 Then

                            '' Unfinished
                            Dim constrInfo As ConstructorInfo = Nothing
                            For Each constrItem As ParseNode In constrs
                                LoadToken(ILgen, constrItem, locals, err)
                                If err = True Then Return
                            Next


                            ILgen.Emit(OpCodes.Newobj, constrInfo)
                            ILgen.Emit(OpCodes.Stloc, local.BaseLocal)
                        Else
                            Console.Error.WriteLine("Error: Invalid amount of parameters.")
                            err = True
                            Return
                        End If
                        Return

                End Select
        End Select
    End Sub
    Private Sub LoadToken(ByVal ILgen As ILGenerator, ByVal value As ParseNode, ByRef locals As List(Of LocalBuilderEx), ByRef Err As Boolean, Optional ByRef local As LocalBuilderEx = Nothing)
        Select Case value.Token.Type
            Case TokenType.IDENTIFIER
                Dim idnt = locals.Find(Function(it) it.VariableName = value.Token.Text)

                If idnt Is Nothing Then
                    Err = True
                    Console.Error.WriteLine("Error: Variable '{0}' doesn't exist in this context.", value.Token.Text)
                    Return
                End If

                If Not local Is Nothing Then
                    local.VariableType = idnt.VariableType
                End If

                ILgen.Emit(OpCodes.Ldloc, idnt.BaseLocal)
                Exit Select
            Case TokenType.NUMBER
                If Not local Is Nothing Then local.VariableType = GetType(Integer)

                ILgen.Emit(OpCodes.Ldc_I4, Integer.Parse(value.Token.Text))
                Exit Select
            Case TokenType.FLOAT
                If Not local Is Nothing Then local.VariableType = GetType(Single)

                ILgen.Emit(OpCodes.Ldc_R4, Single.Parse(value.Token.Text))
                Exit Select
            Case TokenType.NULLKW
                If Not local Is Nothing Then local.VariableType = Nothing

                ILgen.Emit(OpCodes.Ldnull)
                Exit Select
            Case TokenType.STRING
                If Not local Is Nothing Then local.VariableType = GetType(String)

                Dim str As String = value.Token.Text
                'trims first leading and trailing quote marks.
                str = str.Substring(1)
                str = str.Remove(str.Length - 1)

                ILgen.Emit(OpCodes.Ldstr, str)
                Exit Select
            Case TokenType.FuncCall
                HandleFunctionCall(value, ILgen, locals, Err)
                Exit Select
            Case Else
                'If all else fails, declare it as a regular object.

                If Not local Is Nothing Then local.VariableType = GetType(Object)
                ILgen.Emit(OpCodes.Initobj, GetType(Object))
                Exit Select
        End Select
    End Sub
    Private Sub LoadOperator(ByVal op As ParseNode, ByVal ILgen As ILGenerator)
        Select Case op.Token.Type
            Case TokenType.EQUAL
                ILgen.Emit(OpCodes.Ceq)
                Return
            Case TokenType.GT
                ILgen.Emit(OpCodes.Cgt)
                Return
            Case TokenType.LT
                ILgen.Emit(OpCodes.Clt)
                Return
            Case TokenType.NOTEQUAL
                ILgen.Emit(OpCodes.Ceq)
                ILgen.Emit(OpCodes.Ldc_I4_0)
                ILgen.Emit(OpCodes.Ceq)
                Return
        End Select
    End Sub
    Private Function GetLineAndCol(ByVal src As String, ByVal pos As Integer) As Object
        Dim line = 0, col = 0

        Dim eo As New Object
        FindLineAndCol(src, pos, line, col)

        eo.Line = line
        eo.Col = col
        Return eo
    End Function
    Private Sub FindLineAndCol(ByVal src As String, ByVal pos As Integer, ByRef line As Integer, col As Integer)
        ' http://www.codeproject.com/Messages/3852786/Re-ParseError-line-numbers-always-0.aspx

        line = 1
        col = 0

        For i As Integer = 0 To pos
            If (src(i) = vbNewLine) Then
                line += 1
                col = 1
            Else
                col += 1
            End If
        Next
    End Sub
End Module
