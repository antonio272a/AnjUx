using System.Reflection;

namespace AnjUx.Client.Shared
{

    [TransformEnum(typeof(Radzen.JustifyContent))]
    public enum JustifyContent : int
    {
        Normal = 0,
        Center = 1,
        Start = 2,
        End = 3,
        Left = 4,
        Right = 5,
        SpaceBetween = 6,
        SpaceAround = 7,
        SpaceEvenly = 8,
        Stretch = 9,
    }

    [TransformEnum(typeof(Radzen.AlignItems))]
    public enum AlignItems : int
    {
        Normal = 0,
        Center = 1,
        Start = 2,
        End = 3,
        Stretch = 4,
    }

    [TransformEnum(typeof(Radzen.ButtonSize))]
    public enum ButtonSize : int
    {
        Medium = 0,
        Large = 1,
        Small = 2,
        ExtraSmall = 3,
    }

    [TransformEnum(typeof(Radzen.ButtonType))]
    public enum ButtonType : int
    {
        Button = 0,
        Submit = 1,
        Reset = 2,
    }

    [TransformEnum(typeof(Radzen.ButtonStyle))]
    public enum ButtonStyle : int
    {
        Primary = 0,
        Secondary = 1,
        Light = 2,
        Dark = 3,
        Success = 4,
        Danger = 5,
        Warning = 6,
        Info = 7,
    }

    [TransformEnum(typeof(Radzen.Orientation))]
    public enum Orientation : int
    {
        Horizontal = 0,
        Vertical = 1,
    }

    [TransformEnum(typeof(Radzen.IconStyle))]
    public enum IconStyle : int
    {
        Primary = 0,
        Secondary = 1,
        Light = 2,
        Dark = 3,
        Success = 4,
        Danger = 5,
        Warning = 6,
        Info = 7,
    }

    [TransformEnum(typeof(Radzen.TextAlign))]
    public enum TextAlign : int
    {
        Left = 0,
        Right = 1,
        Center = 2,
        Justify = 3,
        JustifyAll = 4,
        Start = 5,
        End = 6,
    }

    [TransformEnum(typeof(Radzen.Density))]
    public enum Density : int
    {
        Default = 0,
        Compact = 1,
    }

    [TransformEnum(typeof(Radzen.DataGridGridLines))]
    public enum DataGridGridLines : int
    {
        Default = 0,
        Both = 1,
        None = 2,
        Horizontal = 3,
        Vertical = 4,
    }

    public enum ToolbarType: int
    {
        ToolBar = 0,
        ActionMenu = 1,
    }

    [AttributeUsage(AttributeTargets.Enum)]
    internal class TransformEnumAttribute(Type tipo) : Attribute
    {
        public Type Tipo = tipo;
    }

    public static class EnumTranslator
    {
        public static Enum TranslateEnum<TSource>(TSource sourceEnum)
            where TSource : Enum
        {
            // Get the source enum type
            var sourceType = typeof(TSource);

            // Check if the source enum type has the TransformEnumAttribute
            var attribute = sourceType.GetCustomAttribute(typeof(TransformEnumAttribute)) ?? throw new InvalidOperationException($"{typeof(TSource).Name} does not have a TransformEnumAttribute");

            Type tipo = ((TransformEnumAttribute)attribute).Tipo;

            // Get the name of the source enum value
            var sourceName = Enum.GetName(sourceType, sourceEnum);

            if (!Enum.TryParse(tipo, sourceName, out var targetEnum))
                throw new InvalidOperationException($"No matching value in {tipo.Name} for {sourceName}");
            
            return (Enum)targetEnum;
        }
    }
}
