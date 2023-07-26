using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FOp.CsvWriter
{
class CsvWriter : IDisposable
{
    private StreamWriter _outstream = null;
    private readonly char _separator;
    private readonly char _quote;

    public CsvWriter(string filename, char separator = ';', char quote = '"', Encoding encoding = null, string lineending = "\r\n")
    {
        if (String.IsNullOrWhiteSpace(filename))
        {
            throw new ArgumentNullException("filename");
        }
        if (null == encoding)
        {
            encoding = Encoding.UTF8;
        }
        if (separator == quote)
        {
            throw new ArgumentException("separator equal to quote");
        }
        _outstream = new StreamWriter(filename, false, encoding)
        {
            NewLine = lineending
        };
        _separator = separator;
        _quote = quote;
    }

    public CsvWriter(Stream stream, char separator = ';', char quote = '"', Encoding encoding = null, string lineending = "\r\n")
    {
        if (null == stream)
        {
            throw new ArgumentNullException("stream");
        }
        if (!stream.CanWrite)
        {
            throw new ArgumentException("stream not writeable");
        }
        if (null == encoding)
        {
            encoding = Encoding.UTF8;
        }
        if (separator == quote)
        {
            throw new ArgumentException("separator equal to quote");
        }
        _outstream = new StreamWriter(stream, encoding)
        {
            NewLine = lineending
        };
        _separator = separator;
        _quote = quote;
    }

#if NET45_OR_GREATER || NETSTANDARD
    public CsvWriter(Stream stream, bool leaveopen = false, char separator = ';', char quote = '"', Encoding encoding = null, string lineending = "\r\n")
    {
        if (null == stream)
        {
            throw new ArgumentNullException("stream");
        }
        if (!stream.CanWrite)
        {
            throw new ArgumentException("stream not writeable");
        }
        if (null == encoding)
        {
            encoding = Encoding.UTF8;
        }
        if (separator == quote)
        {
            throw new ArgumentException("separator equal to quote");
        }
        _outstream = new StreamWriter(stream, encoding, 4096, leaveopen)
        {
            NewLine = lineending
        };
        _separator = separator;
        _quote = quote;
    }
#endif

    public void Close()
    {
        if (_outstream != null)
        {
            _outstream.Close();
            _outstream = null;
        }
    }

    public void Dispose()
    {
        Close();
    }

    public void WriteLine(IEnumerable<string> data)
    {
        bool first = true;
        string eins = new string(_quote, 1);
        string zwei = new string(_quote, 2);
        foreach (string element in data)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                _outstream?.Write(_separator);
            }
            if (!String.IsNullOrEmpty(element))
            {
                bool mustquote = Char.IsWhiteSpace(element[0]) || Char.IsWhiteSpace(element[element.Length - 1]);
                if (!mustquote)
                {
                    foreach (char c in element)
                    {
                        if ((c == _quote) || (c == _separator) || (_outstream.NewLine.IndexOf(c) > -1) || Char.IsControl(c))
                        {
                            mustquote = true;
                            break;
                        }
                    }
                }
                if (mustquote)
                {
                    _outstream.Write(_quote);
                    _outstream.Write(element.Replace(eins, zwei));
                    _outstream.Write(_quote);
                }
                else
                {
                    _outstream.Write(element);
                }
            }
        }
        _outstream.WriteLine();
    }

    public void WriteLines(IEnumerable<IEnumerable<string>> data)
    {
        foreach (IEnumerable<string> line in data)
        {
            WriteLine(line);
        }
    }
}
}
