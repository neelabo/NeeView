﻿using System.Collections.Generic;

namespace NeeView
{
    [Documentable(DocumentType = typeof(CommandAccessor))]
    public interface ICommandAccessor
    {
        string Name { get; }
        bool IsShowMessage { get; set; }
        string MouseGesture { get; set; }
        PropertyMap? Parameter { get; }
        string ShortCutKey { get; set; }
        string TouchGesture { get; set; }

        bool Execute(params object[] args);
        CommandAccessor? Patch(IDictionary<string, object> patch);
    }
}
