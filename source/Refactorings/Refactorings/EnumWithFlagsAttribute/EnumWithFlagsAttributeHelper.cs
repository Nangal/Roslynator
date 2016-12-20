﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslynator.CSharp.Refactorings.EnumWithFlagsAttribute
{
    internal static class EnumWithFlagsAttributeHelper
    {
        public static bool IsEnumWithFlagsAttribute(ITypeSymbol typeSymbol, SemanticModel semanticModel)
        {
            return typeSymbol?.IsEnum() == true
                && typeSymbol
                    .GetAttributes()
                    .Any(f => f.AttributeClass.Equals(semanticModel.Compilation.GetTypeByMetadataName(MetadataNames.System_FlagsAttribute)));
        }

        public static List<object> GetExplicitValues(
            EnumDeclarationSyntax enumDeclaration,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var values = new List<object>();

            foreach (EnumMemberDeclarationSyntax member in enumDeclaration.Members)
            {
                EqualsValueClauseSyntax equalsValue = member.EqualsValue;

                if (equalsValue != null)
                {
                    ExpressionSyntax value = equalsValue.Value;

                    if (value != null)
                    {
                        var fieldSymbol = semanticModel.GetDeclaredSymbol(member, cancellationToken) as IFieldSymbol;

                        if (fieldSymbol?.HasConstantValue == true)
                            values.Add(fieldSymbol.ConstantValue);
                    }
                }
            }

            return values;
        }

        internal static bool TryGetNewValue(
            List<object> values,
            INamedTypeSymbol enumSymbol,
            ValueMode mode,
            out object result)
        {
            if (values.Count == 0)
            {
                result = 0;
                return true;
            }

            switch (enumSymbol.EnumUnderlyingType.SpecialType)
            {
                case SpecialType.System_SByte:
                    {
                        switch (mode)
                        {
                            case ValueMode.UseAllAvailableValues:
                                {
                                    sbyte i = 1;

                                    while (i > 0)
                                    {
                                        if (!values.Cast<sbyte>().Contains(i))
                                        {
                                            result = i;
                                            return true;
                                        }

                                        unchecked
                                        {
                                            i *= 2;
                                        }
                                    }

                                    break;
                                }
                            case ValueMode.StartFromHighestExplicitValue:
                                {
                                    sbyte i = values
                                        .Cast<sbyte>()
                                        .Where(f => f >= 0 && (f & (f - 1)) == 0)
                                        .Max();

                                    unchecked
                                    {
                                        i *= 2;
                                    }

                                    if (i >= 0)
                                    {
                                        result = i;
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case SpecialType.System_Byte:
                    {
                        switch (mode)
                        {
                            case ValueMode.UseAllAvailableValues:
                                {
                                    byte i = 1;

                                    while (i > 0)
                                    {
                                        if (!values.Cast<byte>().Contains(i))
                                        {
                                            result = i;
                                            return true;
                                        }

                                        unchecked
                                        {
                                            i *= 2;
                                        }
                                    }

                                    break;
                                }
                            case ValueMode.StartFromHighestExplicitValue:
                                {
                                    byte i = values
                                        .Cast<byte>()
                                        .Where(f => f >= 0 && (f & (f - 1)) == 0)
                                        .Max();

                                    unchecked
                                    {
                                        i *= 2;
                                    }

                                    if (i >= 0)
                                    {
                                        result = i;
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case SpecialType.System_Int16:
                    {
                        switch (mode)
                        {
                            case ValueMode.UseAllAvailableValues:
                                {
                                    short i = 1;

                                    while (i > 0)
                                    {
                                        if (!values.Cast<short>().Contains(i))
                                        {
                                            result = i;
                                            return true;
                                        }

                                        unchecked
                                        {
                                            i *= 2;
                                        }
                                    }

                                    break;
                                }
                            case ValueMode.StartFromHighestExplicitValue:
                                {
                                    short i = values
                                        .Cast<short>()
                                        .Where(f => f >= 0 && (f & (f - 1)) == 0)
                                        .Max();

                                    unchecked
                                    {
                                        i *= 2;
                                    }

                                    if (i >= 0)
                                    {
                                        result = i;
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case SpecialType.System_UInt16:
                    {
                        switch (mode)
                        {
                            case ValueMode.UseAllAvailableValues:
                                {
                                    ushort i = 1;

                                    while (i > 0)
                                    {
                                        if (!values.Cast<ushort>().Contains(i))
                                        {
                                            result = i;
                                            return true;
                                        }

                                        unchecked
                                        {
                                            i *= 2;
                                        }
                                    }

                                    break;
                                }
                            case ValueMode.StartFromHighestExplicitValue:
                                {
                                    ushort i = values
                                        .Cast<ushort>()
                                        .Where(f => f >= 0 && (f & (f - 1)) == 0)
                                        .Max();

                                    unchecked
                                    {
                                        i *= 2;
                                    }

                                    if (i >= 0)
                                    {
                                        result = i;
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case SpecialType.System_Int32:
                    {
                        switch (mode)
                        {
                            case ValueMode.UseAllAvailableValues:
                                {
                                    int i = 1;

                                    while (i > 0)
                                    {
                                        if (!values.Cast<int>().Contains(i))
                                        {
                                            result = i;
                                            return true;
                                        }

                                        unchecked
                                        {
                                            i *= 2;
                                        }
                                    }

                                    break;
                                }
                            case ValueMode.StartFromHighestExplicitValue:
                                {
                                    int i = values
                                        .Cast<int>()
                                        .Where(f => f >= 0 && (f & (f - 1)) == 0)
                                        .Max();

                                    unchecked
                                    {
                                        i *= 2;
                                    }

                                    if (i >= 0)
                                    {
                                        result = i;
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case SpecialType.System_UInt32:
                    {
                        switch (mode)
                        {
                            case ValueMode.UseAllAvailableValues:
                                {
                                    uint i = 1;

                                    while (i > 0)
                                    {
                                        if (!values.Cast<uint>().Contains(i))
                                        {
                                            result = i;
                                            return true;
                                        }

                                        unchecked
                                        {
                                            i *= 2;
                                        }
                                    }

                                    break;
                                }
                            case ValueMode.StartFromHighestExplicitValue:
                                {
                                    uint i = values
                                        .Cast<uint>()
                                        .Where(f => f >= 0 && (f & (f - 1)) == 0)
                                        .Max();

                                    unchecked
                                    {
                                        i *= 2;
                                    }

                                    if (i >= 0)
                                    {
                                        result = i;
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case SpecialType.System_Int64:
                    {
                        switch (mode)
                        {
                            case ValueMode.UseAllAvailableValues:
                                {
                                    long i = 1;

                                    while (i > 0)
                                    {
                                        if (!values.Cast<long>().Contains(i))
                                        {
                                            result = i;
                                            return true;
                                        }

                                        unchecked
                                        {
                                            i *= 2;
                                        }
                                    }

                                    break;
                                }
                            case ValueMode.StartFromHighestExplicitValue:
                                {
                                    long i = values
                                        .Cast<long>()
                                        .Where(f => f >= 0 && (f & (f - 1)) == 0)
                                        .Max();

                                    unchecked
                                    {
                                        i *= 2;
                                    }

                                    if (i >= 0)
                                    {
                                        result = i;
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case SpecialType.System_UInt64:
                    {
                        switch (mode)
                        {
                            case ValueMode.UseAllAvailableValues:
                                {
                                    ulong i = 1;

                                    while (i > 0)
                                    {
                                        if (!values.Cast<ulong>().Contains(i))
                                        {
                                            result = i;
                                            return true;
                                        }

                                        unchecked
                                        {
                                            i *= 2;
                                        }
                                    }

                                    break;
                                }
                            case ValueMode.StartFromHighestExplicitValue:
                                {
                                    ulong i = values
                                        .Cast<ulong>()
                                        .Where(f => f >= 0 && (f & (f - 1)) == 0)
                                        .Max();

                                    unchecked
                                    {
                                        i *= 2;
                                    }

                                    if (i >= 0)
                                    {
                                        result = i;
                                        return true;
                                    }

                                    break;
                                }
                        }

                        break;
                    }
            }

            result = null;
            return false;
        }
    }
}