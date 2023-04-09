using AutoMapper;
using System.Reflection;

namespace Services.Mapping
{

    public static class ConfigMapper
    {
        public static IMapper Instance { get; private set; }

        public static void RegisterMappings(params Assembly[] assemblies) => Instance = new Mapper(new MapperConfiguration(cfg =>
        { cfg.AddMaps(assemblies); }));
    }
}
