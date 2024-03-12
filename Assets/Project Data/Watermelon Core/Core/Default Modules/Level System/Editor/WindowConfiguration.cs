#pragma warning disable 649
using UnityEngine;

namespace Watermelon
{
    [System.Serializable] // this is needed to survive acembly reload;
    public sealed class WindowConfiguration
    {
        private string windowTitle;
        private bool keepWindowOpenOnScriptReload;
        private bool restrictWindowMinSize;
        private Vector2 windowMinSize;
        private bool restrictWindowMaxSize;
        private Vector2 windowMaxSize;
        private bool restrictContentMaxSize;
        private Vector2 contentMaxSize;
        private bool restictContentHeight;

        public bool KeepWindowOpenOnScriptReload { get => keepWindowOpenOnScriptReload; }
        public bool RestrictWindowMinSize { get => restrictWindowMinSize; }
        public Vector2 WindowMinSize { get => windowMinSize; set => windowMinSize = value; }
        public bool RestrictWindowMaxSize { get => restrictWindowMaxSize; }
        public Vector2 WindowMaxSize { get => windowMaxSize; set => windowMaxSize = value; }
        public string WindowTitle => windowTitle;
        public bool RestrictContentMaxSize => restrictContentMaxSize;
        public Vector2 ContentMaxSize => contentMaxSize;
        public bool RestictContentHeight => restictContentHeight;

        private WindowConfiguration()
        {
            this.windowTitle = LevelEditorBase.DEFAULT_LEVEL_EDITOR_TITLE;
            this.windowMinSize = Vector2.one * LevelEditorBase.DEFAULT_WINDOW_MIN_SIZE;
        }


        public sealed class Builder
        {
            private WindowConfiguration editorConfiguration;

            public Builder()
            {
                editorConfiguration = new WindowConfiguration();
            }

            public Builder SetWindowTitle(string windowTitle)
            {
                editorConfiguration.windowTitle = windowTitle;
                return this;
            }

            public Builder KeepWindowOpenOnScriptReload(bool keepWindowOpenOnScriptReload)
            {
                editorConfiguration.keepWindowOpenOnScriptReload = keepWindowOpenOnScriptReload;
                return this;
            }

            public Builder SetWindowMinSize(Vector2 windowMinSize)
            {
                editorConfiguration.windowMinSize = windowMinSize;
                editorConfiguration.restrictWindowMinSize = true;
                return this;
            }

            public Builder SetWindowMaxSize(Vector2 windowMaxSize)
            {
                editorConfiguration.windowMaxSize = windowMaxSize;
                editorConfiguration.restrictWindowMaxSize = true;
                return this;
            }

            public Builder SetContentMaxSize(Vector2 contentMaxSize)
            {
                editorConfiguration.contentMaxSize = contentMaxSize;
                editorConfiguration.restrictContentMaxSize = true;
                return this;
            }

            public Builder RestrictContentHeight(bool enable)
            {
                editorConfiguration.restictContentHeight = enable;
                return this;
            }

            public WindowConfiguration Build()
            {
                return editorConfiguration;
            }

        }
    }
}
