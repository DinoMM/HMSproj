using CommunityToolkit.Mvvm.ComponentModel;
using DBLayer;
using DBLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace RecepciaModul.ViewModels
{
    public partial class HostConFlagyViewModel : ObservableObject
    {
        public List<HostConFlag> ZoznamFlagovHost = new();
        public List<HostConFlag> ZoznamFlagovHostPred = new();

        public List<HostFlag> ZoznamVsetkychFlagov = new();

        public Host? ReadOnlyHost { get; set; }

        public bool NacitavaniePoloziek { get; private set; } = true;

        private readonly DBContext _db;
        private readonly UserService _userService;

        public HostConFlagyViewModel(DBContext db, UserService userService)
        {
            _db = db;
            _userService = userService;
        }

        public async Task NacitajZoznamy()
        {
            if (ReadOnlyHost == null)
            {
                return;
            }

            ZoznamVsetkychFlagov = new(_db.HostFlags
                .OrderBy(x => x.ID)
                .ToList());
            ZoznamFlagovHost.AddRange(_db.HostConFlags
                .Include(x => x.HostFlagX)
                .Include(x => x.HostX)
                .Where(x => x.Host == ReadOnlyHost.ID)
                .OrderBy(x => x.HostFlag)
                .ToList());
            ZoznamFlagovHostPred.AddRange(ZoznamFlagovHost);

            ZoznamFlagovHost.ForEach(x => ZoznamVsetkychFlagov.Remove(x.HostFlagX));

            NacitavaniePoloziek = false;
        }

        public void Delete(HostConFlag flag)
        {
            ZoznamFlagovHost.Remove(flag);
            ZoznamVsetkychFlagov.Add(flag.HostFlagX);
            ZoznamVsetkychFlagov.Sort((x, y) => x.ID.CompareTo(y.ID));
        }

        public void SpracujNovyFlag(List<HostFlag> flags)
        {
            foreach (var flag in flags)
            {
                if (ZoznamFlagovHost.FirstOrDefault(x => x.HostFlag == flag.ID) == null)
                {
                    ZoznamFlagovHost.Add(new HostConFlag() { Host = ReadOnlyHost.ID, HostFlag = flag.ID, HostFlagX = flag });

                    ZoznamVsetkychFlagov.Remove(flag);
                }
            }
        }

        public void Ulozit()
        {
            foreach (var flag in ZoznamFlagovHost)
            {
                var found = ZoznamFlagovHostPred.FirstOrDefault(x => x.HostFlag == flag.HostFlag);
                if (found == null)
                {
                    _db.HostConFlags.Add(flag);
                }
                else
                {
                    ZoznamFlagovHostPred.Remove(found);
                }
            }

            foreach (var flag in ZoznamFlagovHostPred)
            {
                if (ZoznamFlagovHost.FirstOrDefault(x => x.HostFlag == flag.HostFlag) == null)
                {
                    _db.HostConFlags.Remove(flag);
                }
            }
            ZoznamFlagovHostPred.Clear();
            ZoznamFlagovHostPred.AddRange(ZoznamFlagovHost);
            _db.SaveChanges();
        }

    }
}