using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using gbfr.discord.richpresence.Template.Configuration;
using Reloaded.Mod.Interfaces.Structs;

namespace gbfr.discord.richpresence.Configuration
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Enable Discord Rich Presence")]
        [Description("Whether to enable Discord Rich Presence/Activity.")]
        [DefaultValue(true)]
        public bool EnableDiscordRichPresence { get; set; } = true;

        [DisplayName("Use startup time for elapsed time counter")]
        [Description("If disabled, the counter will start from 0 whenever it updates, like switching areas or entering new quests.")]
        [DefaultValue(false)]
        public bool UseStartupTimeTimestamp { get; set; } = false;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
