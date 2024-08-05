using System.Text.RegularExpressions;
using NCalc;
using System.Globalization;
namespace CuraEngineParametersLibrary;

public class ParametersParser
{
    private bool HasExpressionMark(string value)
    { 
        if ((value.Contains(" if ") && value.Contains(" else ")) || value.Contains("len(") || value.Contains("sum(") || value.Contains("Ceiling(") ||
            value.Contains("Floor(") || value.Contains("Log(") || value.Contains("Sqrt(") || value.Contains("Pi(") ||
            value.Contains("Cos(") || value.Contains("Tan(") || value.Contains("Sin(") || value.Contains("Radians(") ||
            value.Contains("Min(") ||value.Contains("Max(") || value.Contains("Round(") || 
            value.Contains("extruderValueFromContainer(") || value.Contains("valueFromContainer(") || 
            value.Contains("defaultExtruderPosition(") || value.Contains("resolveOrValue(") || 
            value.Contains("anyExtruderNrWithOrDefault(") || value.Contains("anyExtruderWithMaterial(") || 
            value.Contains("extruderValues(") || value.Contains("extruderValue(") || value.Contains("in (") || value.Contains("all("))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool DoubleTryParse(string valueStr, out double resultDouble)
    {
        return double.TryParse(valueStr.Replace(",", "."), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out resultDouble);
    }
    private bool IsExpression(string value, ParameterType paramType, bool checkParamNameSymbol = true)
    {
        bool resultBool;
        int resultInt;
        double resultDouble;
        if (bool.TryParse(value, out resultBool) ||
            int.TryParse(value, out resultInt) ||
            DoubleTryParse(value, out resultDouble) ||
            value.StartsWith("[") && value.EndsWith("]") && !value.Contains("if") ||
            value=="")
        {
            return false;
        }
        else if ((checkParamNameSymbol && value.Contains("_")) ||  (paramType!=ParameterType.ptStr && (value.Contains("+") ||
                 value.Contains("-") ||  value.Contains("*") ||  value.Contains(" / "))) ||
                 value.Contains(" != ") ||  value.Contains(" > ") ||  value.Contains(" < ") ||
                 value.Contains("==") ||  value.Contains(" or ") ||  value.Contains(" and ") ||
                 value.Contains(" not ") || value.StartsWith("not ") ||
                 HasExpressionMark(value))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private Expression GetExpressionWithAdditionalFunctions(string value)
    {
        Expression expression = new Expression(value);
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args)
        {
            if (name == "Round" && args.Parameters.Length==1)
            {
                args.Result = Math.Round((double)args.Parameters[0].Evaluate());
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args)
        {
            if (name == "Radians" && args.Parameters.Length==1)
            {
                double value;
                DoubleTryParse(args.Parameters[0].Evaluate().ToString(), out value);
                args.Result = value*0.0174533;
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args)
        {
            if (name == "resolveOrValue" && args.Parameters.Length==1)
            {
                var val = args.Parameters[0].Evaluate().ToString();
                double resultDouble;
                if (DoubleTryParse(val, out resultDouble))
                {         
                    args.Result = resultDouble;
                }
                else
                {
                    args.Result = val;
                }
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args)
        {
            if ((name == "Max" || name == "Min") && args.Parameters.Length==1)
            {
                var val = args.Parameters[0].Evaluate().ToString();
                val = val.Replace(",", ".");
                double doubleVal;
                DoubleTryParse(val, out doubleVal);
                args.Result = doubleVal;
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args)
        {
            if (name == "Max" && args.Parameters.Length>2)
            {
                var count = args.Parameters.Length;
                var maxArg = Double.MinValue;
                for (int i = 0; i < count; i ++)
                {
                    double arg;
                    DoubleTryParse(args.Parameters[i].Evaluate().ToString(), out arg);
                    if (arg > maxArg)
                    {
                        maxArg = arg;
                    }
                }
                args.Result = maxArg;
            }
        };
        expression.EvaluateParameter += delegate(string name, ParameterArgs args)
        {
            if (name == "Pi")
            {
                args.Result = 3.14;
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args)
        {
            if (name == "Min" && args.Parameters.Length>2)
            {
                var count = args.Parameters.Length;
                var minArg = Double.MaxValue;
                for (int i = 0; i < count; i ++)
                {
                    double arg;
                    DoubleTryParse(args.Parameters[i].Evaluate().ToString(), out arg);
                    if (arg < minArg)
                    {
                        minArg = arg;
                    }
                }
                args.Result = minArg;
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args) // two arguments, the first indicates which extruder to use (we only have one)
        {
            if (name == "extruderValue" && args.Parameters.Length==1)
            {
                var expression = args.Parameters[0].Evaluate().ToString().Replace("'", "");
                double val;
                if (DoubleTryParse(expression, out val))
                {
                    args.Result = val;
                }
                else
                {
                    args.Result = expression;
                }
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args) // two arguments, the first indicates which extruder to use (we only have one)
        {
            if (name == "extruderValue" && args.Parameters.Length==2)
            {
                var expression = args.Parameters[1].Evaluate().ToString().Replace("'", "");
                double val;
                if (DoubleTryParse(expression, out val))
                {
                    args.Result = val;
                }
                else
                {
                    args.Result = expression;
                }
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args) // as I understand it, it is necessary if there are two extruders
        {
            if (name == "map" && args.Parameters.Length==2)
            {
                var expression = args.Parameters[1].Evaluate().ToString().Replace("'", "");
                double val;
                if (DoubleTryParse(expression, out val))
                {
                    args.Result = val;
                }
                else
                {
                    args.Result = expression;
                }
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args)
        {
            if (name == "extruderValues" && args.Parameters.Length==1)
            {
                var expression = args.Parameters[0].Evaluate().ToString().Replace("'", "");
                double val;
                if (DoubleTryParse(expression, out val))
                {
                    args.Result = val;
                }
                else
                {
                    args.Result = expression;
                }
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args) //wherever anyExtruderWithMaterial occurs, we completely replace the entire value with 0, even if there are conditions (everywhere it occurs, the extruder number is needed, we have one)
        {
            if (name == "anyExtruderWithMaterial" && args.Parameters.Length==1)
            {
                args.Result = 0;
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args) // since there is only one extruder, we discard the sum function
        {
            if (name == "sum" && args.Parameters.Length==1)
            {
                var expression = args.Parameters[0].Evaluate().ToString().Replace("'", "");
                double val;
                if (DoubleTryParse(expression, out val))
                {
                    args.Result = val;
                }
                else
                {
                    args.Result = expression;
                }
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args) // since there is only one extruder, we disable the any function
        {
            if (name == "any" && args.Parameters.Length==1)
            {
                var expression = args.Parameters[0].Evaluate().ToString().Replace("'", "");
                double val;
                if (DoubleTryParse(expression, out val))
                {
                    args.Result = val;
                }
                else
                {
                    args.Result = expression;
                }
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args) // returns the number of extruders that have this parameter. since there is only one extruder, we always return 1 (for example, there is a layer_start_x calculation, which is calculated from the sum of the parameter from all printer extruders divided by the number)
        {
            if (name == "len" && args.Parameters.Length==1)
            {
                args.Result = 1;
            }
        };
        expression.EvaluateFunction+= delegate(string name, FunctionArgs args)
        {
            if (name == "int" && args.Parameters.Length==1)
            {
                var expression = args.Parameters[0].Evaluate().ToString();
                if (expression.Contains("defaultExtruderPosition"))
                {
                   args.Result = 0; 
                }
                else
                {
                    var eval = args.Parameters[0].Evaluate();
                    args.Result = Convert.ToInt32(eval);
                }
                
            }
        };
        return expression;
    }
    private string MaxFromArray(string value)
    {
        while (value.Contains("Max(["))
        {
            var maxFirstInd = value.LastIndexOf("Max([");
            var maxSecondInd = value.IndexOf("])", maxFirstInd);
            var sourceMaxString = value.Substring(maxFirstInd, maxSecondInd-maxFirstInd+2);
            var valuesString = sourceMaxString.Replace("Max([", "");
            valuesString = valuesString.Replace("])", "");
            var values = valuesString.Split(",");
            double maxValue = double.MinValue;
            foreach (string valueFromString in values)
            {
                double doubleValue;
                Expression expression = GetExpressionWithAdditionalFunctions(valueFromString.Trim());
                try
                {
                    if (!expression.HasErrors())
                    {
                        var result = expression.Evaluate().ToString();
                        DoubleTryParse(result, out doubleValue);
                        if (doubleValue>maxValue)
                        {
                            maxValue = doubleValue; 
                        }
                    }
                    
                }
                catch (EvaluationException ex)
                {
                    Console.WriteLine($"Couldn't calculate the value: {ex.Message}");           
                }
            }
            if (maxValue != double.MinValue)
            {
                value = value.Replace(sourceMaxString, maxValue.ToString());
            }
        }
        return value;
    }
    private string MinFromArray(string value)
    {
        while (value.Contains("Min(["))
        {
            var minFirstInd = value.LastIndexOf("Min([");
            var minSecondInd = value.IndexOf("])", minFirstInd);
            var sourceMinString = value.Substring(minFirstInd, minSecondInd-minFirstInd+2);
            var valuesString = sourceMinString.Replace("Min([", "");
            valuesString = valuesString.Replace("])", "");
            var values = valuesString.Split(",");
            double minValue = double.MaxValue;
            foreach (string valueFromString in values)
            {
                double doubleValue;
                Expression expression = GetExpressionWithAdditionalFunctions(valueFromString.Trim());
                try
                {
                    if (!expression.HasErrors())
                    {
                        var result = expression.Evaluate().ToString();;
                        DoubleTryParse(result, out doubleValue);
                        if (doubleValue<minValue)
                        {
                            minValue = doubleValue; 
                        }
                    }
                    
                }
                catch (EvaluationException ex)
                {
                    Console.WriteLine($"Couldn't calculate the value: {ex.Message}");           
                }
            }
            if (minValue != double.MinValue)
            {
                value = value.Replace(sourceMinString, minValue.ToString());
            }
        }
        return value;
    }
    private string FormatAll(string value)
    {
        string result = value;
        string pattern = @"all\((.+?) for (.+?) in (.+?)\)";
        Match match = Regex.Match(value, pattern);

        if (match.Success)
        {
            string expression = match.Groups[1].Value;
            string variable = match.Groups[2].Value;

            string replacedVariable = match.Groups[3].Value.Trim() + ")";
            string replacedCondition = expression.Replace(variable, replacedVariable);

            string prefix = value.Substring(0, match.Index);
            result = prefix + replacedCondition;
        }
        return result;
    }
    private string FormatIn(string value)
    {
        string subStringValue = "";
        if (value.Contains(" not in "))
        {
            var index = value.IndexOf(" not in ");
            var subString = value.Substring(0, index);
            if (subString.LastIndexOf("if(")!=-1)
            {
                var indexIf = subString.LastIndexOf("if(");
                subStringValue = subString.Substring(indexIf+3, subString.Length-indexIf-3).Trim();
                value = value.Replace("if(" + subStringValue, "if(");
            }
            else
            {
                int ind = subString.Trim().LastIndexOf(" ");
                if (ind>-1)
                {
                    subString = subString.Substring(ind+1, subString.Length-ind-1);
                    value = value.Remove(ind+1, subString.Length);
                }       
                else
                {
                    value = value.Remove(0, subString.Length);
                }             
                subStringValue = subString;
             //   value = value.Substring(subStringValue.Length);
            }  
            value = value.Replace(" not in (", "not in (" + subStringValue + ", ");
        }
        var indexIn = value.IndexOf(" in ");
        var subStringBeforeIn = value.Substring(0, indexIn+1);
        if (!subStringBeforeIn.EndsWith("not ") && !subStringBeforeIn.StartsWith("not "))
        {
            var index = value.IndexOf(" in ");
            var subString = value.Substring(0, index);
            if (subString.LastIndexOf("if(")!=-1)
            {
                var indexIf = subString.LastIndexOf("if(");
                subStringValue = subString.Substring(indexIf+3, subString.Length-indexIf-3).Trim();
                value = value.Replace("if(" + subStringValue, "if(");
            }
            else
            {
                subStringValue = subString;
                value = value.Substring(subStringValue.Length);
            }
            value = value.Replace(" in (", " in (" + subStringValue + ", "); 
        }
        return value;
    }
    static bool IsOperator(string token)
    {
        return token == "+" || token == "-" || token == "*" || token == "/";
    }
    private string ReplaceBooleanVariableIfNeed(string value)
    {
        string[] tokens = value.Split(' ');

        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].ToLower() == "false" && (i > 0 && IsOperator(tokens[i - 1]) || (i < tokens.Length - 1 && IsOperator(tokens[i + 1]))))
            {
                tokens[i] = "0";
            }
            if (tokens[i].ToLower() == "true" && (i > 0 && IsOperator(tokens[i - 1]) || (i < tokens.Length - 1 && IsOperator(tokens[i + 1]))))
            {
                tokens[i] = "1";
            }
        }

        string result = string.Join(" ", tokens);
        return result;
    }
    private int CheckIndexOfIf(int indexOfIF, string value)
    {
        var nextIndexOfIf = value.IndexOf(" if ", indexOfIF + 4);
        var indexOfElse = value.IndexOf(" else ", indexOfIF);
        if (nextIndexOfIf == -1)
        {
            return indexOfIF;
        }
        if (nextIndexOfIf < indexOfElse)
        {
            nextIndexOfIf = value.Substring(0, indexOfElse).LastIndexOf(" if ");
            if (nextIndexOfIf != -1)
            {
                indexOfIF = nextIndexOfIf;
            }
        }
        return indexOfIF;
    }
    private string FormatStringBranch(string branch)
    {
        if (branch.StartsWith("'") && branch.EndsWith("'"))
        {
            branch = branch.Substring(1, branch.Length-2);
            branch = branch.Replace("'", "\\'");
            branch = "'" + branch + "'";
        }
        return branch;
    }
    private bool CheckIfCondition(string value)
    {
        var indexOfIF = value.IndexOf(" if ");
        if (indexOfIF==-1)
        {
            return false;
        }
        var indexOfElse = value.IndexOf(" else ", indexOfIF);
        return (indexOfIF != -1 && indexOfElse != -1 && indexOfIF<indexOfElse);
    }
    private string FormatConditionsWithoutParentheses(string value)
    {        
        int openParentheseCount = 0;
        int closeParentheseCount = 0;

        var indexOfIF = value.IndexOf(" if ");
        if (indexOfIF != -1)
        {
            indexOfIF = CheckIndexOfIf(indexOfIF, value);
            var trueBranch = "";
            var subStringBeforeIF = value.Substring(0, indexOfIF);
            var subStringAfterIF = value.Substring(indexOfIF+4, value.Length-indexOfIF-4);
            var indexOfELSE = subStringBeforeIF.IndexOf(" else ");
            var indexOfLastComma = subStringBeforeIF.LastIndexOf(",");
            var replacedSubString = "";
            if (indexOfELSE != -1)
            {
                var subString = subStringBeforeIF.Substring(indexOfELSE);
                for (int i = 0; i < subString.Length; i++)
                {
                    if (subString[i] == ')')
                    {
                        closeParentheseCount++;
                    }
                    if (subString[i] == '(')
                    {
                        openParentheseCount++;
                    }
                }
                if (closeParentheseCount>openParentheseCount) //if ")" is greater than "(" then this condition does not belong to the "else" before it, then it is everything before it trueBranch
                {
                    trueBranch = value.Substring(0, indexOfIF);
                    replacedSubString = value;
                }
                else
                {
                    trueBranch = subStringBeforeIF.Substring(indexOfELSE + 6);
                    replacedSubString = value.Substring(indexOfELSE + 6);
                } 
            }
            else
            {
                var isPartOfAnotherFunction = false;
                var indOfOpenCondition = 0;
                openParentheseCount = 0;
                closeParentheseCount = 0;
                for (int i = indexOfIF; i >= 0; i--)
                {
                    if (value[i] == '(' && (i==0 || value[i-1] != ' ')) //checking if the "if-else" condition belongs to another function, example extruderValue(0 if true else 1, 3)
                    {
                        indOfOpenCondition = i;
                        trueBranch = value.Substring(i+1, indexOfIF-i-1);
                        isPartOfAnotherFunction = true;
                    }
                    if (value[i] == ')')
                    {
                        break;
                    }
                }
                if (isPartOfAnotherFunction)
                {
                    var indOfOEndCondition = value.IndexOf(",", indOfOpenCondition);
                    replacedSubString = value.Substring(indOfOpenCondition+1, indOfOEndCondition-indOfOpenCondition-1);
                    indexOfIF = replacedSubString.IndexOf(" if ");
                    subStringAfterIF = replacedSubString.Substring(indexOfIF+4, replacedSubString.Length-indexOfIF-4);
                }
                else
                {
                    trueBranch = value.Substring(0, indexOfIF);
                    replacedSubString = value;  
                }
                
            }
            indexOfELSE = subStringAfterIF.IndexOf(" else ");
            var conditionBranch = subStringAfterIF.Substring(0, indexOfELSE);
            var falseBranch = subStringAfterIF.Substring(indexOfELSE + 6, subStringAfterIF.Length-indexOfELSE-6);
            if (trueBranch.Contains(" if ") && trueBranch.Contains(" else "))
            {
                trueBranch = FormatConditionsWithoutParentheses(trueBranch);
            }
            if (falseBranch.Contains(" if ") && falseBranch.Contains(" else "))
            {
                falseBranch = FormatConditionsWithoutParentheses(falseBranch);
            }
            trueBranch = FormatStringBranch(trueBranch);
            falseBranch = FormatStringBranch(falseBranch);
            var formatedIF = "(if(" + conditionBranch + ", " + trueBranch + " else " + falseBranch + "))";
            value = value.Replace(replacedSubString, formatedIF);
        //    value = FormatConditionsWithoutParentheses(value);
        }
        if (CheckIfCondition(value))
        {
            value = FormatConditionsWithoutParentheses(value);
        }
        value = value.Replace(" else ", ", ");
        return value;
    }
    private string FormatConditionsWithParentheses(string value, int startInd)
    {
        int openParentheseInd = -1;
        int closeParentheseInd = -1;
        int openParentheseCount = 0;
        int closeParentheseCount = 0;
        int endOfIF = -1;
        value = value.Substring(startInd);
        var indexOfIF = value.IndexOf(" if ");
        if (indexOfIF != -1)
        {
            for (int i = indexOfIF; i >= 0; i--)
            {
                if (value[i] == ')')
                {
                    closeParentheseCount++;
                }
                if (value[i] == '(')// && (i==0 || value[i-1] == ' ')) //checking if the "if-else" condition does not belong to another function, example extruderValue(0 if true else 1, 3)
                {
                    openParentheseCount++;
                }
                if (openParentheseCount>closeParentheseCount) //if true, then "(" belongs to "if"
                {
                    if (i!=0 && value[i-1] != ' ')
                    {
                        break;
                    }
                    openParentheseInd = i;

                    closeParentheseCount = 0;
                    openParentheseCount = 0;
                    for (int j = indexOfIF; j < value.Length; j++)
                    {
                        if (value[j] == ')')
                        {
                            closeParentheseCount++;
                        }
                        if (value[j] == '(')
                        {
                            openParentheseCount++;
                        }
                        if (closeParentheseCount>openParentheseCount) //if true, then ")" belongs to "if"
                        {
                            closeParentheseInd = j;
                            var parenthesesSubString = value.Substring(openParentheseInd, closeParentheseInd-openParentheseInd+1);
                            indexOfIF = parenthesesSubString.IndexOf(" if ");
                            var indexOfELSE = parenthesesSubString.IndexOf(" else ");
                            var trueBranch = parenthesesSubString.Substring(1, indexOfIF-1);
                            var conditionBranch = parenthesesSubString.Substring(indexOfIF+4, indexOfELSE-indexOfIF-4);
                            var falseBranch = parenthesesSubString.Substring(indexOfELSE+6, parenthesesSubString.Length-indexOfELSE-7);
                            if (trueBranch.Contains(" if "))
                            {
                                trueBranch = FormatConditionsWithParentheses(trueBranch, 0);
                            }
                            if (falseBranch.Contains(" if "))
                            {
                                falseBranch = FormatConditionsWithParentheses(falseBranch, 0);
                            }
                            var formatedIF = "(if(" + conditionBranch + ", " + trueBranch + " else " + falseBranch + "))";
                            value = value.Replace(parenthesesSubString, formatedIF);
                            endOfIF = value.IndexOf(formatedIF) + formatedIF.Length;
                          //  value = FormatConditionsWithParentheses(value, 0);
                            break;
                        }
                    }
                }
                if (openParentheseInd != -1 && closeParentheseInd != -1)
                {
                    break;
                }
            }
            if (openParentheseInd == -1 || closeParentheseInd == -1)
            {
                var subString = value.Substring(0, indexOfIF+4);
                var formatedString = FormatConditionsWithParentheses(value, indexOfIF+4);
                value = subString + formatedString;
            }
        }
        if (endOfIF != -1)
        {
            var subStringAfterIF = value.Substring(endOfIF);
            var subStringBeforeIF = value.Substring(0, endOfIF);
            if (subStringAfterIF.Contains(" if "))
            {
                subStringAfterIF = FormatConditionsWithParentheses(subStringAfterIF, 0);
                value = subStringBeforeIF + subStringAfterIF;
            }      
        }
        return value;
    }
    private string FormatConditions(string value)
    {
        value = FormatConditionsWithParentheses(value, 0);
        value = FormatConditionsWithoutParentheses(value);
        return value;
    }    
    
    private string CalculateValue(string value, Dictionary<string, Parameter> CurrentParams, string keyElement, ParameterValueType valueType, bool IsSaveCalculatedValue)
    {
        if (value!="")
        {
            value = FormatValue(value, CurrentParams[keyElement].paramType);
            if (value.Contains("Max(["))
            {
                value = MaxFromArray(value);
            }
            if (value.Contains("Min(["))
            {
                value = MaxFromArray(value);
            }
            if (value.Contains("all("))
            {
                value = FormatAll(value);
            }
            if (CheckIfCondition(value))
            {
                value = value.Replace("[", "'[");
                value = value.Replace("]", "]'");
                value = FormatConditions(value);
            }
            if (value.Contains(" in "))
            {
                value = FormatIn(value);
            }
            value = ReplaceBooleanVariableIfNeed(value);
            Expression expression = GetExpressionWithAdditionalFunctions(value);
            try
            {
                if (!expression.HasErrors())
                {
                    object result = expression.Evaluate();
                    value = Convert.ToString(result);
                    value = value.Replace(",", ".");            
                }
                
            }
            catch (EvaluationException ex)
            {
                Console.WriteLine($"Couldn't calculate the value: {ex.Message}");           
            }
            if (IsSaveCalculatedValue)
            {
                CurrentParams[keyElement].SetValueByType(value, valueType);
            }
        }       
        return value;
    }
    public string FormatValue(string value, ParameterType paramType)
    {
        bool resultBool;
        double resultDouble;
        if (DoubleTryParse(value, out resultDouble))
        {
            if (value.StartsWith("-"))
            {
                value = "(" + value + ")";
            }
        }
        else if (!bool.TryParse(value, out resultBool) && !value.StartsWith("(") && !value.StartsWith("'") && !IsExpression(value, paramType))
        {
            value = "'" + value + "'";
        }
        value = value.Replace("''", "'");
        return value;
    }
    private string ReplaceValue(Dictionary<string, Parameter> CurrentParams, Dictionary<string, Parameter> SecondParams, string keyElement, string valueElement, ParameterValueType valueType, bool IsSaveCalculatedValue)
    {
        string pattern = $@"\b\w*{"_"}\w*\b";
        MatchCollection matches = Regex.Matches(valueElement, pattern);
        var hasInParams = true;
        var result = CurrentParams[keyElement].GetValueByType(valueType);
        for (int i = 0; i < matches.Count; i++)
        {
            var value = "";
            Parameter param;
            var key = matches[i].Value;
            hasInParams = CurrentParams.TryGetValue(key, out param);
            if (hasInParams)
            {
                value = param.value;
                if (IsExpression(value, param.paramType))
                {
                    value = ReplaceValue(CurrentParams, SecondParams, key, value, ParameterValueType.pvtValue, IsSaveCalculatedValue);
                }
                value = FormatValue(value, param.paramType);
                result = Regex.Replace(result, $@"\b{Regex.Escape(key)}\b", value);                              
            }
            else if (CurrentParams!=SecondParams)
            {
                hasInParams = SecondParams.TryGetValue(key, out param);
                if (hasInParams)
                {
                    value = param.value;
                    if (IsExpression(value, param.paramType))
                    {
                        value = ReplaceValue(SecondParams, SecondParams, key, value, ParameterValueType.pvtValue, IsSaveCalculatedValue);
                    }
                    value = FormatValue(value, param.paramType);
                    result = Regex.Replace(result, $@"\b{Regex.Escape(key)}\b", value);       
                }
            }
        }
        if (IsSaveCalculatedValue)
        {
            CurrentParams[keyElement].SetValueByType(result, valueType);   
        }
        if (matches.Count == 1 && !hasInParams) //most likely this is a ready-made value (enumerated), example "outside_in"
        {
            if (result.Contains("_") && !result.StartsWith("'"))
            {
                result = "'"+result+"'";
            }
            return result;
        }
        else if (IsExpression(result, CurrentParams[keyElement].paramType, false))
        {
            //for debug
            // if (keyElement=="machine_start_gcode")
            // {
            //     var stop = true;
            // }
            result = CalculateValue(result, CurrentParams, keyElement, valueType, IsSaveCalculatedValue);
            if (IsSaveCalculatedValue)
            {
                CurrentParams[keyElement].SetValueByType(result, valueType);   
            }
            return result;
        }
        else
        {
            return result;  
        }    
    }

    public bool ParseParameters(bool IsGlobalParams, ref Dictionary<string, Parameter> _GlobalParams, ref Dictionary<string, Parameter> _ExtruderParams)
    {
        Dictionary<string, Parameter> CurrentParams;
        Dictionary<string, Parameter> SecondParams;
        if (IsGlobalParams)
        {
            CurrentParams = _GlobalParams;
            SecondParams = _ExtruderParams;
        }
        else
        {
            CurrentParams = _ExtruderParams;
            SecondParams = _GlobalParams;
        }
        foreach (KeyValuePair<string, Parameter> keyValueElement in CurrentParams)
        {
            Console.WriteLine(keyValueElement.Key + "=" + keyValueElement.Value.value);
            ParseParameter(keyValueElement.Key, ref CurrentParams, ref SecondParams, true);        
        }

        return true;
    }

    public string ParseParameter(string ParameterName, ref Dictionary<string, Parameter> CurrentParams, ref Dictionary<string, Parameter> SecondParams, bool IsSaveCalculatedValue)
    {
        Parameter parameter = CurrentParams[ParameterName];
        var result = "";
        //for debug
        // if (ParameterName=="machine_start_gcode")
        // {
        //     var stop = true;
        // }
        if (IsExpression(parameter.value, parameter.paramType) && ParameterName!="material_guid")
        {
            var value = parameter.value;
            result = ReplaceValue(CurrentParams, SecondParams, ParameterName, value, ParameterValueType.pvtValue, false);
        }
        else
        {
            result = CurrentParams[ParameterName].value;
        }
        result = result.Replace("true", "True");
        result = result.Replace("false", "False");
        result = result.Replace("'", "");
 
        if (IsSaveCalculatedValue)
        {
            CurrentParams[ParameterName].calculatedValue = result;
        }
        if (parameter.paramType==ParameterType.ptFloat)
            result = result.Replace(",", ".");   
        return result;
    }

    public bool IsParameterEnabled(string ParameterName, ref Dictionary<string, Parameter> CurrentParams, ref Dictionary<string, Parameter> SecondParams)
    {
        Parameter parameter = CurrentParams[ParameterName];
        if (IsExpression(parameter.enabled, ParameterType.ptBool))
        {
            var value = parameter.enabled;
            return Convert.ToBoolean(ReplaceValue(CurrentParams, SecondParams, ParameterName, value, ParameterValueType.pvtEnabledValue, false));
        }
        return Convert.ToBoolean(parameter.enabled);
    }
    public double GetMinimumValue(string ParameterName, ref Dictionary<string, Parameter> CurrentParams, ref Dictionary<string, Parameter> SecondParams)
    {
        Parameter parameter = CurrentParams[ParameterName];
        var value = "";
        if (parameter.HasMinimumValue && IsExpression(parameter.minimumValue, parameter.paramType))
        {
            value = parameter.minimumValue;
            value = ReplaceValue(CurrentParams, SecondParams, ParameterName, value, ParameterValueType.pvtMinimumValue, false);
        }
        value = value.Replace(",", ".");
        double doubleValue;
        DoubleTryParse(value, out doubleValue);
        return doubleValue;
    }
    public double GetMinimumValueWarning(string ParameterName, ref Dictionary<string, Parameter> CurrentParams, ref Dictionary<string, Parameter> SecondParams)
    {
        Parameter parameter = CurrentParams[ParameterName];
        var value = "";
        if (parameter.HasMinimumValueWarning && IsExpression(parameter.minimumValueWarning, parameter.paramType))
        {
            value = parameter.minimumValueWarning;
            value = ReplaceValue(CurrentParams, SecondParams, ParameterName, value, ParameterValueType.pvtMinimumValueWarning, false);
        }
        value = value.Replace(",", ".");
        double doubleValue;
        DoubleTryParse(value, out doubleValue);
        return doubleValue;
    }
    public double GetMaximumValue(string ParameterName, ref Dictionary<string, Parameter> CurrentParams, ref Dictionary<string, Parameter> SecondParams)
    {
        Parameter parameter = CurrentParams[ParameterName];
        var value = "";
        if (parameter.HasMaximumValue && IsExpression(parameter.maximumValue, parameter.paramType))
        {
            value = parameter.maximumValue;
            value = ReplaceValue(CurrentParams, SecondParams, ParameterName, value, ParameterValueType.pvtMaximumValue, false);
        }
        value = value.Replace(",", ".");
        double doubleValue;
        DoubleTryParse(value, out doubleValue);
        return doubleValue;
    }
    public double GetMaximumValueWarning(string ParameterName, ref Dictionary<string, Parameter> CurrentParams, ref Dictionary<string, Parameter> SecondParams)
    {
        Parameter parameter = CurrentParams[ParameterName];
        var value = "";
        if (parameter.HasMaximumValueWarning && IsExpression(parameter.maximumValueWarning, parameter.paramType))
        {
            value = parameter.maximumValueWarning;
            value = ReplaceValue(CurrentParams, SecondParams, ParameterName, value, ParameterValueType.pvtMaximumValueWarning, false);
        }
        value = value.Replace(",", ".");
        double doubleValue;
        DoubleTryParse(value, out doubleValue);
        return doubleValue;
    }
}