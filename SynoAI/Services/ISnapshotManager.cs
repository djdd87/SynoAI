using System;
using System.IO;
using SynoAI.Models;

namespace SynoAI.Services
{
    public interface ISnapshotManager
    {
        ProcessedImage GetImage(Camera camera);
    }
}