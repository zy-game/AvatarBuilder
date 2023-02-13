mergeInto(LibraryManager.library, {
    OnNotiflyEvent:function (name, data) {
        name = UTF8ToString(name)
        if (name != "SHOW_OPEN_FILE_DIALOG") {
            data = UTF8ToString(data)
            window.ReportReady(name, data)
            return;
        }
       
    },
    OnSelectionFile:function(){
        let input = window.document.createElement("input");
        input.value = "选择文件";
        input.type = "file";
        input.onchange = event => {
            let file = event.target.files[0];
            window.gameInstance.SendMessage("WebGLAvatar", "OpenFileCallback", URL.createObjectURL(file));
        };
        input.click();
    },
    glClear: function(mask)
    {
        if (mask == 0x00004000)
        {
            var v = GLctx.getParameter(GLctx.COLOR_WRITEMASK);
            if (!v[0] && !v[1] && !v[2] && v[3])
                // We are trying to clear alpha only -- skip.
                return;
        }
        GLctx.clear(mask);
    },
});