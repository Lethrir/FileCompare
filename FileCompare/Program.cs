using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace FileCompare
{
    class Program
    {
        private static List<FileDescriptor> _files;

        static void Main(string[] args)
        {
            var start = DateTime.Now;
            _files = new List<FileDescriptor>();

            DoDirectory("P:\\");

            ReportProgress();

            ReportStats();

            Console.WriteLine("Done searching.");
            var end = DateTime.Now;
            Console.WriteLine(end - start);

            Console.ReadKey();

            var orderedFiles = _files.Where(f=>f.Locations.Count > 1).OrderByDescending(f => f.Size);
            //var i = 0;
            foreach (var fd in orderedFiles)
            {

                //if(i==100)
                //{
                //    Console.ReadKey();
                //    i = 0;
                //}
                //else i++;

                Console.WriteLine(string.Format("Duplicated file {0} {1} times at size {2}", fd.Name, fd.Locations.Count, fd.Size));
                var locs = fd.Locations.Take(fd.Locations.Count - 1);
                foreach (var loc in locs)
                {
                    //Console.WriteLine("Delete?");
                    //if(Console.ReadKey().KeyChar == 'Y')
                    //{
                        File.Delete(loc + fd.Name);
                        //Console.WriteLine("DELETED");
                    //}
                }
            }

            Console.WriteLine("Done deleting.");
            Console.ReadKey();
        }

        private static void DoDirectory(string path)
        {
            Console.WriteLine(path);
            ReportProgress();
            var dirs = Directory.GetDirectories(path);
            
            foreach (var dir in dirs)
            {
                DoDirectory(dir);
            }

            var files = Directory.GetFiles(path);
            DoFiles(files);

        }

        private static void DoFiles(string[] files)
        {
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                
                Func<FileDescriptor, bool> p = f => f.Name == GetName(file) && f.Size == info.Length;

                if(_files.Any(p))
                {
                    var fd = _files.First(p);
                    fd.Locations.Add(GetPath(file));
                } else
                {
                    var fd = new FileDescriptor {Name = GetName(file), Size = info.Length};
                    fd.Locations.Add(GetPath(file));
                    _files.Add(fd);
                }
            }
        }

        private static string GetPath(string file)
        {
            var i = file.LastIndexOf('\\');
            var path = file.Substring(0, i+1);
            return path;
        }

        private static string GetName(string file)
        {
            var i = file.LastIndexOf('\\');
            var name = file.Substring(i+1, file.Length - i-1);
            return name;
        }

        private static void ReportProgress()
        {
            Console.WriteLine("Files: " + _files.Count);
            Console.WriteLine("Duplicates: " + _files.Where(f => f.Locations.Count > 1).Sum(f=>f.Locations.Count - 1));
        }

        private static void ReportStats()
        {
            long fs = 0;
            var dups = _files.Where(f => f.Locations.Count > 1);
            foreach (var fileDescriptor in dups)
            {
                fs += fileDescriptor.Size*(fileDescriptor.Locations.Count - 1);
            }

            Console.WriteLine("Space to free: " + ToMB(fs) + " MB.");

            fs = 0;

            Console.WriteLine("Total unique data: " + ToMB(_files.Sum(fd => fd.Size)) + " MB");
        }

        private static long ToMB(long fs)
        {
            return fs/1024/1024;
        }

        private class FileDescriptor
        {
            private List<string> _locations;
            
            public string Name { get; set; }
            public long Size { get; set; }
            public List<string> Locations { get { return _locations; } } 

            public FileDescriptor()
            {
                _locations = new List<string>();
            }
        }
    }
}
