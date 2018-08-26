#region Copyright (c) 2007 by Dan Shechter

////////////////////////////////////////////////////////////////////////////////////////
////
//  IBNet, an Interactive Brokers TWS .NET Client & Server implmentation
//  by Dan Shechter
////////////////////////////////////////////////////////////////////////////////////////
//  License: MPL 1.1/GPL 2.0/LGPL 2.1
//  
//  The contents of this file are subject to the Mozilla Public License Version 
//  1.1 (the "License"); you may not use this file except in compliance with 
//  the License. You may obtain a copy of the License at 
//  http://www.mozilla.org/MPL/
//  
//  Software distributed under the License is distributed on an "AS IS" basis,
//  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
//  for the specific language governing rights and limitations under the
//  License.
//  
//  The Original Code is any part of this file that is not marked as a contribution.
//  
//  The Initial Developer of the Original Code is Dan Shecter.
//  Portions created by the Initial Developer are Copyright (C) 2007
//  the Initial Developer. All Rights Reserved.
//  
//  Contributor(s): None.
//  
//  Alternatively, the contents of this file may be used under the terms of
//  either the GNU General Public License Version 2 or later (the "GPL"), or
//  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
//  in which case the provisions of the GPL or the LGPL are applicable instead
//  of those above. If you wish to allow use of your version of this file only
//  under the terms of either the GPL or the LGPL, and not to allow others to
//  use your version of this file under the terms of the MPL, indicate your
//  decision by deleting the provisions above and replace them with the notice
//  and other provisions required by the GPL or the LGPL. If you do not delete
//  the provisions above, a recipient may use your version of this file under
//  the terms of any one of the MPL, the GPL or the LGPL.
////////////////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Diagnostics;
using System.IO;

namespace Daemaged.IBNet
{
  /// <summary>
  /// A Buffered Read Stream that is safe for use with network sockets
  /// This stream wraps a given Stream (e.g. NetStream) and buffers
  /// the Read() and ReadByte operations. This in turn increases the performance
  /// of client reading partial data off a network socket.
  /// The recv() system call is called only once to fill up as much as possible
  /// data into the buffers, and subsequent Read/ReadByte() calls are served from the buffer.
  /// NO buffering what so ever is performed for write operations.
  /// </summary>
  internal class BufferedReadStream : Stream
  {
    const int DEFAULT_BUFFER_SIZE = 4096;
    readonly int _bufferSize; // Length of internal buffer, if it's allocated.
    byte[] _buffer; // Shared read buffer, allocated in lazy fashion
    int _readLen; // Number of bytes read in buffer from _s.
    int _readPos; // Read pointer within shared buffer.
    Stream _s; // Underlying stream

    public BufferedReadStream(Stream stream)
      : this(stream, DEFAULT_BUFFER_SIZE) {}

    public BufferedReadStream(Stream stream, int bufferSize)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (bufferSize <= 0)
        throw new ArgumentOutOfRangeException("bufferSize", "buffer Size must be positive");

      if (stream is FileStream)
        throw new ArgumentException("FileStream is buffered - it is sensless to buffer it", "stream");
      if (stream is MemoryStream)
        throw new ArgumentException("it is sensless to buffer MemoryStream", "stream");

      _s = stream;
      _bufferSize = bufferSize;

      if (!_s.CanRead && !_s.CanWrite)
        Error_StreamIsClosed();
    }

    public override bool CanRead
    {
      get { return _s != null && _s.CanRead; }
    }

    public override bool CanWrite
    {
      get { return _s != null && _s.CanWrite; }
    }

    public override bool CanSeek
    {
      get { return _s != null && _s.CanSeek; }
    }

    public override long Length
    {
      get
      {
        if (_s == null) Error_StreamIsClosed();
        return _s.Length;
      }
    }

    public override long Position
    {
      get
      {
        if (_s == null)
          Error_StreamIsClosed();
        if (!_s.CanSeek)
          Error_SeekNotSupported();
        return _s.Position + _readPos - _readLen;
      }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "Position should be zero or positive");
        if (_s == null)
          Error_StreamIsClosed();
        if (!_s.CanSeek)
          Error_SeekNotSupported();
        _readPos = 0;
        _readLen = 0;
        _s.Seek(value, SeekOrigin.Begin);
      }
    }

    protected override void Dispose(bool disposing)
    {
      try {
        if (disposing && _s != null) {
          try {
            Flush();
          }
          finally {
            _s.Close();
          }
        }
      }
      finally {
        _s = null;
        _buffer = null;

        base.Dispose(disposing);
      }
    }

    public override void Flush()
    {
      if (_s == null)
        Error_StreamIsClosed();

      _s.Flush();
      if (_readPos < _readLen && _s.CanSeek) {
        FlushRead();
      }
    }

    // Reading is done by blocks from the file, but someone could read
    // 1 byte from the buffer then write.  At that point, the OS's file
    // pointer is out of sync with the stream's position.  All write 
    // functions should call this function to preserve the position in the file.
    void FlushRead()
    {
      if (_readPos - _readLen != 0)
        _s.Seek(_readPos - _readLen, SeekOrigin.Current);
      _readPos = 0;
      _readLen = 0;
    }

    public override int Read(byte[] array, int offset, int count)
    {
      if (array == null)
        throw new ArgumentNullException("array", "array is null");
      if (offset < 0)
        throw new ArgumentOutOfRangeException("offset", "must be non-negative");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", "must be non-negative");
      if (array.Length - offset < count)
        throw new ArgumentOutOfRangeException("offset", "offset is invalid with respect to array length and count");

      if (_s == null)
        Error_StreamIsClosed();

      var n = _readLen - _readPos;
      // if the read buffer is empty, read into either user's array or our
      // buffer, depending on number of bytes user asked for and buffer size.
      if (n == 0) {
        if (!_s.CanRead)
          Error_ReadNotSupported();

        // When reading larger the _bufferSize reads we directly
        // call the underlyinh stream's Read() and reset the current buffer...
        if (count >= _bufferSize) {
          n = _s.Read(array, offset, count);
          // Throw away read buffer.
          _readPos = 0;
          _readLen = 0;
          return n;
        }

        // Need to allocate?
        if (_buffer == null)
          _buffer = new byte[_bufferSize];

        n = _s.Read(_buffer, 0, _bufferSize);
        if (n == 0)
          return 0;
        _readPos = 0;
        _readLen = n;
      }

      // Now copy min of count or numBytesAvailable (ie, near EOF) to array.
      if (n > count)
        n = count;
      Buffer.BlockCopy(_buffer, _readPos, array, offset, n);
      _readPos += n;

      if (n < count) {
        var moreBytesRead = _s.Read(array, offset + n, count - n);
        n += moreBytesRead;
        _readPos = 0;
        _readLen = 0;
      }

      return n;
    }

    // Reads a byte from the underlying stream.  Returns the byte cast to an int
    // or -1 if reading from the end of the stream.
    public override int ReadByte()
    {
      if (_s == null)
        Error_StreamIsClosed();
      if (_readLen == 0 && !_s.CanRead)
        Error_ReadNotSupported();
      if (_readPos == _readLen) {
        if (_buffer == null)
          _buffer = new byte[_bufferSize];
        _readLen = _s.Read(_buffer, 0, _bufferSize);
        _readPos = 0;
      }
      if (_readPos == _readLen)
        return -1;

      return _buffer[_readPos++];
    }

    public override void Write(byte[] array, int offset, int count)
    {
      _s.Write(array, offset, count);
    }


    public override void WriteByte(byte value)
    {
      _s.WriteByte(value);
    }


    public override long Seek(long offset, SeekOrigin origin)
    {
      if (_s == null)
        Error_StreamIsClosed();
      if (!_s.CanSeek)
        Error_SeekNotSupported();
      // If we've got bytes in our buffer to write, write them out.
      // If we've read in and consumed some bytes, we'll have to adjust
      // our seek positions ONLY IF we're seeking relative to the current
      // position in the stream.
      Debug.Assert(_readPos <= _readLen, "_readPos <= _readLen");
      if (origin == SeekOrigin.Current) {
        // Don't call FlushRead here, which would have caused an infinite
        // loop.  Simply adjust the seek origin.  This isn't necessary
        // if we're seeking relative to the beginning or end of the stream.
        Debug.Assert(_readLen - _readPos >= 0, "_readLen (" + _readLen + ") - _readPos (" + _readPos + ") >= 0");
        offset -= (_readLen - _readPos);
      }
      /*
            _readPos = 0;
            _readLen = 0;
            return _s.Seek(offset, origin);
            */
      var oldPos = _s.Position + (_readPos - _readLen);
      var pos = _s.Seek(offset, origin);

      // We now must update the read buffer.  We can in some cases simply
      // update _readPos within the buffer, copy around the buffer so our 
      // Position property is still correct, and avoid having to do more 
      // reads from the disk.  Otherwise, discard the buffer's contents.
      if (_readLen > 0) {
        // We can optimize the following condition:
        // oldPos - _readPos <= pos < oldPos + _readLen - _readPos
        if (oldPos == pos) {
          if (_readPos > 0) {
            //Console.WriteLine("Seek: seeked for 0, adjusting buffer back by: "+_readPos+"  _readLen: "+_readLen);
            Buffer.BlockCopy(_buffer, _readPos, _buffer, 0, _readLen - _readPos);
            _readLen -= _readPos;
            _readPos = 0;
          }
          // If we still have buffered data, we must update the stream's 
          // position so our Position property is correct.
          if (_readLen > 0)
            _s.Seek(_readLen, SeekOrigin.Current);
        }
        else if (oldPos - _readPos < pos && pos < oldPos + _readLen - _readPos) {
          var diff = (int) (pos - oldPos);
          //Console.WriteLine("Seek: diff was "+diff+", readpos was "+_readPos+"  adjusting buffer - shrinking by "+ (_readPos + diff));
          Buffer.BlockCopy(_buffer, _readPos + diff, _buffer, 0, _readLen - (_readPos + diff));
          _readLen -= (_readPos + diff);
          _readPos = 0;
          if (_readLen > 0)
            _s.Seek(_readLen, SeekOrigin.Current);
        }
        else {
          // Lose the read buffer.
          _readPos = 0;
          _readLen = 0;
        }
        Debug.Assert(_readLen >= 0 && _readPos <= _readLen,
                     "_readLen should be nonnegative, and _readPos should be less than or equal _readLen");
        Debug.Assert(pos == Position, "Seek optimization: pos != Position!  Buffer math was mangled.");
      }
      return pos;
    }

    public override void SetLength(long value)
    {
      if (value < 0)
        throw new ArgumentOutOfRangeException("value", "must not be negative");
      if (_s == null)
        Error_StreamIsClosed();
      if (!_s.CanSeek)
        Error_SeekNotSupported();
      if (!_s.CanWrite)
        Error_WriteNotSupported();

      if (_readPos < _readLen) {
        FlushRead();
      }
      _s.SetLength(value);
    }

    void Error_StreamIsClosed()
    {
      throw new IOException("The underlying stream is closed");
    }

    void Error_SeekNotSupported()
    {
      throw new Exception("The underlying stream does not support seeking");
    }

    void Error_ReadNotSupported()
    {
      throw new Exception("The underlying stream does not support reading");
    }

    void Error_WriteNotSupported()
    {
      throw new Exception("The underlying stream does not support writing");
    }
  }
}