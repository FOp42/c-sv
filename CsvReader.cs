using System;
using System.Text;
using System.IO;
using System.Globalization;
using System.Collections;

namespace FOp.CsvReader
{
class CsvReader
{
    private readonly StreamReader FInStream;
    private readonly char FSeparator;
    private readonly char FQuote;
    private char[] FBuffer;
    private int FBufferFill;
    private int FBufferIndex;

    private bool IsNewLine(char c)
    {
        if ((c >= ' ') && (c < '\x007f'))
        {
            return (false);
        }
        if ((c == '\n') || (c == '\r'))
        {
            return (true);
        }
        return (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.LineSeparator);
    }

    public CsvReader(StreamReader InStream, Char Separator = ',', Char Quote = '"')
    {
        if (IsNewLine(Separator))
        {
            throw new Exception("Line separator as separator not allowed");
        }
        if (IsNewLine(Quote))
        {
            throw new Exception("Line separator as quotes not allowed");
        }
        if (Separator == Quote)
        {
            throw new Exception("Separator equals quotes");
        }
        this.FInStream = InStream;
        this.FSeparator = Separator;
        this.FQuote = Quote;
        this.FBuffer = new char[8192];
        this.FBufferFill = 0;
        this.FBufferIndex = 0;
    }

    private bool Read(out char derda)
    {
        if (this.FBufferIndex < this.FBufferFill)
        {
            derda = this.FBuffer[this.FBufferIndex];
            this.FBufferIndex++;
            return (true);
        }
        else
        {
            if (this.FInStream.EndOfStream)
            {
                derda = '\0';
                return (false);
            }
            this.FBufferFill = this.FInStream.Read(this.FBuffer, 0, this.FBuffer.Length);
            if (0 == this.FBufferFill)
            {
                derda = '\0';
                return (false);
            }
            derda = this.FBuffer[0];
            this.FBufferIndex = 1;
            return (true);
        }
    }

    private bool Peek(out char derda)
    {
        if (this.FBufferIndex < this.FBufferFill)
        {
            derda = this.FBuffer[this.FBufferIndex];
            return (true);
        }
        else
        {
            if (this.FInStream.EndOfStream)
            {
                derda = '\0';
                return (false);
            }
            this.FBufferFill = this.FInStream.Read(this.FBuffer, 0, this.FBuffer.Length);
            if (0 == this.FBufferFill)
            {
                derda = '\0';
                return (false);
            }
            derda = this.FBuffer[0];
            this.FBufferIndex = 0;
            return (true);
        }
    }

    public bool NotEndOfStream()
    {
        if (this.FBufferIndex < this.FBufferFill)
        {
            return (true);
        }
        else
        {
            if (this.FInStream.EndOfStream)
            {
                return (false);
            }
            this.FBufferFill = this.FInStream.Read(this.FBuffer, 0, this.FBuffer.Length);
            if (0 == this.FBufferFill)
            {
                return (false);
            }
            this.FBufferIndex = 0;
            return (true);
        }
    }

    public String[] ReadLine()
    {
        Boolean Quoted;
        ArrayList Result;
        StringBuilder SoFar = new StringBuilder();
        char ach;
        char dann;
        bool doit = false;
        Quoted = false;
        Result = new ArrayList();
        while (Read(out ach))
        {
            doit = true;
            if (Quoted)
            {
                if (ach == this.FQuote)
                {
                    if (Peek(out dann))
                    {
                        if (dann == this.FQuote)
                        {
                            if (Read(out dann))
                            {
                                SoFar.Append(dann);
                            }
                        }
                        else
                        {
                            Quoted = false;
                        }
                    }
                    else
                    {
                        Quoted = false;
                    }
                }
                else
                {
                    SoFar.Append(ach);
                }
            }
            else
            {
                if (ach == this.FQuote)
                {
                    Quoted = true;
                }
                else
                {
                    if ((ach == this.FSeparator) || IsNewLine(ach))
                    {
                        Result.Add(SoFar.ToString());
                        if (ach != this.FSeparator)
                        {
                            /* Zeilenende */
                            if (Peek(out dann))
                            {
                                if (dann != ach)
                                {
                                    if (IsNewLine(dann))
                                    {
                                        Read(out dann);
                                    }
                                }
                            }
                            doit = false;
                            break;
                        }
                        SoFar.Clear();
                    }
                    else
                    {
                        SoFar.Append(ach);
                    }
                }
            }
        }
        if (doit)
        {
            Result.Add(SoFar.ToString());
        }
        return ((String[])(Result.ToArray(typeof(string))));
    }
}
}
