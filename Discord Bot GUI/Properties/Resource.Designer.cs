﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Discord_Bot.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Resource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Discord_Bot.Properties.Resource", typeof(Resource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Admin Commands:
        ///
        ///!help admin		Sends in a txt containing all the commands
        ///!help feature		Sends in a txt containing all features
        ///!command add [command name] [link]		Adding custom commands
        ///!command remove [command name]		Removes custom command with given name
        ///!channel add [type] [channel name]		Set a channel for a given type, some types only allow one per server
        ///!channel remove [type] [channel name]		Unset a channel for a given type, some types only allow one per server
        ///!twitch role add [role name]		Se [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Admin_Commands {
            get {
                return ResourceManager.GetString("Admin_Commands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Features:
        ///
        ///-Replying to messages starting with &quot;I think&quot; in agreement
        ///-Replying to someone saying &quot;I am&quot; or &quot;I&apos;m&quot;
        ///-If someone types &quot;@(bot name)&quot; in a message it replies with a gif
        ///-Replying to pre defined list of keywords
        ///-Adding roles to users from pre-defined list
        ///-Very rare random responses
        ///-Reliable automatic instagram embeds
        ///-Send birthday messages for users that have it set on the day of.
        /// </summary>
        public static string Bot_Features {
            get {
                return ResourceManager.GetString("Bot_Features", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] Kim_Synthji {
            get {
                object obj = ResourceManager.GetObject("Kim_Synthji", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Owner Commands:
        ///
        ///!help owner		Includes owner commands too
        ///!say [channel name] [message]		Sends the message to the channel you want to like the bot wrote it, deletes your message
        ///!biaslist add [name]-[group]		Add bias to global list of biases (DM)
        ///!biaslist remove [name]-[group]		Remove bias from global list of biases (DM)
        ///!bias alias add [alias]-[name]-[group]		Add bias alias to global list of bias aliases (DM)
        ///!bias alias remove [alias]-[name]-[group]		Remove bias alias from global list of bias alia [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Owner_Commands {
            get {
                return ResourceManager.GetString("Owner_Commands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap polaroid_base {
            get {
                object obj = ResourceManager.GetObject("polaroid_base", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Text Commands:
        ///
        ///!help		List of commands (DM)
        ///!8ball [question]		8ball game (DM)
        ///!custom list		List of custom commands
        ///!coin flip [head] or [tails]		Flip a coin (DM)
        ///!decide [opt1],[opt2]...		Decide from options (DM)
        ///!wotd [language]		Word of the day in a language (DM)
        ///!birthday add [date]		Add your birthday for server
        ///!birthday remove		Remove your birthday from server
        ///!birthday list		List all birthdays from server
        ///!bonk [user] [delay(ms)]		bonk
        ///.twt [links]		Twitter embed (DM)
        ///
        ///Remind Commands [rest of string was truncated]&quot;;.
        /// </summary>
        public static string User_Commands {
            get {
                return ResourceManager.GetString("User_Commands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap winner_bracket {
            get {
                object obj = ResourceManager.GetObject("winner_bracket", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap winter0 {
            get {
                object obj = ResourceManager.GetObject("winter0", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        public static System.Drawing.Bitmap winter1 {
            get {
                object obj = ResourceManager.GetObject("winter1", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
