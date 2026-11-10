// Маленький HTTP-сервер только для 127.0.0.1 (TcpListener — без http.sys и без прав администратора).
// Одно соединение = один запрос (Connection: close). Файлы — с Range (перемотка аудио в браузере).
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Req
{
    public string Method, Path, RawQuery;
    public NameValueCollection Q = new NameValueCollection();
    public Dictionary<string, string> H = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public long Length;
    public Stream Body;           // ровно Length байт
    public string Text() { var b = new byte[Length]; int o = 0; while (o < Length) { int n = Body.Read(b, o, (int)Length - o); if (n <= 0) break; o += n; } return Encoding.UTF8.GetString(b, 0, o); }
    public Dictionary<string, object> JsonBody() { string t = Text(); return string.IsNullOrWhiteSpace(t) ? new Dictionary<string, object>() : App.Json.Deserialize<Dictionary<string, object>>(t); }
}

class Resp
{
    readonly Stream s; public bool Sent;
    public Resp(Stream s) { this.s = s; }
    void Head(int code, string type, long len, IDictionary<string, string> extra)
    {
        var sb = new StringBuilder("HTTP/1.1 " + code + " " + (code == 200 ? "OK" : code == 206 ? "Partial Content" : code == 404 ? "Not Found" : code == 416 ? "Range Not Satisfiable" : "Error") + "\r\n");
        sb.Append("Content-Type: " + type + "\r\nContent-Length: " + len + "\r\nConnection: close\r\nCache-Control: no-store\r\nX-Content-Type-Options: nosniff\r\n");
        if (extra != null) foreach (var kv in extra) sb.Append(kv.Key + ": " + kv.Value + "\r\n");
        sb.Append("\r\n");
        byte[] h = Encoding.ASCII.GetBytes(sb.ToString());
        s.Write(h, 0, h.Length); Sent = true;
    }
    public void Bytes(int code, string type, byte[] b) { Head(code, type, b.Length, null); s.Write(b, 0, b.Length); }
    public void Json(object o, int code = 200) { Bytes(code, "application/json; charset=utf-8", Encoding.UTF8.GetBytes(App.Json.Serialize(o))); }
    public void Error(string msg, int code = 500) { Json(new Dictionary<string, object> { { "error", msg } }, code); }
    public void File(string path, Req r)
    {
        if (!System.IO.File.Exists(path)) { Bytes(404, "text/plain", Encoding.ASCII.GetBytes("not found")); return; }
        using (var f = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
        {
            long size = f.Length, a = 0, b = size - 1; bool part = false;
            string rg; r.H.TryGetValue("Range", out rg);
            if (rg != null && rg.StartsWith("bytes="))
            {
                string[] p = rg.Substring(6).Split(',')[0].Split('-');
                if (p[0] != "") { a = long.Parse(p[0]); if (p.Length > 1 && p[1] != "") b = Math.Min(long.Parse(p[1]), size - 1); }
                else if (p.Length > 1 && p[1] != "") a = Math.Max(0, size - long.Parse(p[1]));
                part = true;
                if (a >= size) { Head(416, "text/plain", 0, new Dictionary<string, string> { { "Content-Range", "bytes */" + size } }); return; }
            }
            var extra = new Dictionary<string, string> { { "Accept-Ranges", "bytes" } };
            if (part) extra["Content-Range"] = "bytes " + a + "-" + b + "/" + size;
            Head(part ? 206 : 200, Mime(path), b - a + 1, extra);
            f.Position = a; long left = b - a + 1; var buf = new byte[65536];
            while (left > 0) { int n = f.Read(buf, 0, (int)Math.Min(buf.Length, left)); if (n <= 0) break; s.Write(buf, 0, n); left -= n; }
        }
    }
    static string Mime(string p)
    {
        switch (Path.GetExtension(p).ToLowerInvariant())
        {
            case ".html": return "text/html; charset=utf-8";
            case ".js": return "text/javascript; charset=utf-8";
            case ".css": return "text/css; charset=utf-8";
            case ".json": return "application/json; charset=utf-8";
            case ".png": return "image/png";
            case ".jpg": case ".jpeg": return "image/jpeg";
            case ".svg": return "image/svg+xml";
            case ".wav": return "audio/wav";
            case ".ico": return "image/x-icon";
            default: return "application/octet-stream";
        }
    }
}

class HttpServer
{
    readonly TcpListener l;
    readonly Action<Req, Resp> handler;
    public int Port { get { return ((IPEndPoint)l.LocalEndpoint).Port; } }
    public HttpServer(Action<Req, Resp> h) { handler = h; l = new TcpListener(IPAddress.Loopback, 0); l.Start(); }

    public void Serve()
    {
        while (true)
        {
            TcpClient c;
            try { c = l.AcceptTcpClient(); } catch { return; }
            var t = new Thread(() => One(c)) { IsBackground = true };
            t.Start();
        }
    }
    public void Stop() { try { l.Stop(); } catch { } }

    void One(TcpClient c)
    {
        using (c)
        {
            try
            {
                c.ReceiveTimeout = 120000; c.SendTimeout = 120000;
                var ns = c.GetStream();
                var bs = new BufferedStream(ns, 65536);
                string line = ReadLine(bs); if (string.IsNullOrEmpty(line)) return;
                string[] p = line.Split(' ');
                var r = new Req { Method = p[0] };
                string url = p.Length > 1 ? p[1] : "/";
                int qi = url.IndexOf('?');
                r.Path = Uri.UnescapeDataString(qi >= 0 ? url.Substring(0, qi) : url);
                r.RawQuery = qi >= 0 ? url.Substring(qi + 1) : "";
                foreach (string kv in r.RawQuery.Split('&'))
                {
                    if (kv == "") continue; int e = kv.IndexOf('=');
                    r.Q[Uri.UnescapeDataString(e >= 0 ? kv.Substring(0, e) : kv)] = e >= 0 ? Uri.UnescapeDataString(kv.Substring(e + 1).Replace('+', ' ')) : "";
                }
                while (true)
                {
                    string h = ReadLine(bs); if (string.IsNullOrEmpty(h)) break;
                    int i = h.IndexOf(':'); if (i > 0) r.H[h.Substring(0, i).Trim()] = h.Substring(i + 1).Trim();
                }
                // защита от чужих страниц: запросы только со своего адреса (Host = 127.0.0.1:порт)
                string host; r.H.TryGetValue("Host", out host);
                var resp = new Resp(ns);
                if (host != "127.0.0.1:" + Port && host != "localhost:" + Port) { resp.Bytes(403, "text/plain", Encoding.ASCII.GetBytes("forbidden")); return; }
                string cl; r.Length = r.H.TryGetValue("Content-Length", out cl) ? long.Parse(cl) : 0;
                r.Body = new LimitedStream(bs, r.Length);
                handler(r, resp);
                if (!resp.Sent) resp.Bytes(404, "text/plain", Encoding.ASCII.GetBytes("not found"));
                ns.Flush();
            }
            catch (IOException) { }
            catch (Exception e) { App.Log("http: " + e); }
        }
    }

    static string ReadLine(Stream s)
    {
        var sb = new StringBuilder();
        while (true)
        {
            int b = s.ReadByte();
            if (b < 0) return sb.Length > 0 ? sb.ToString() : null;
            if (b == '\n') break;
            if (b != '\r') sb.Append((char)b);
            if (sb.Length > 16384) throw new IOException("line too long");
        }
        return sb.ToString();
    }
}

class LimitedStream : Stream
{
    readonly Stream s; long left;
    public LimitedStream(Stream s, long n) { this.s = s; left = n; }
    public override int Read(byte[] b, int o, int c) { if (left <= 0) return 0; int n = s.Read(b, o, (int)Math.Min(c, left)); left -= n; return n; }
    public override bool CanRead { get { return true; } }
    public override bool CanSeek { get { return false; } }
    public override bool CanWrite { get { return false; } }
    public override long Length { get { throw new NotSupportedException(); } }
    public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
    public override void Flush() { }
    public override long Seek(long a, SeekOrigin b) { throw new NotSupportedException(); }
    public override void SetLength(long v) { throw new NotSupportedException(); }
    public override void Write(byte[] b, int o, int c) { throw new NotSupportedException(); }
}
