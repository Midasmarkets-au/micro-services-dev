import registerRoute from "./routes";
import registerMenu from "./menu";

const ToolsModule = {
  install: function (app, options) {
    registerRoute(options.router);
    registerMenu(options.menu);
  },
};
export default ToolsModule;
