import registerRoute from "./routes";
import registerMenu from "./menu";

const DocumentsModule = {
  install: function (app, options) {
    registerRoute(options.router);
    registerMenu(options.menu);
  },
};
export default DocumentsModule;
