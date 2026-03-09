import registerMenu from "./menu";
import registerRoute from "./routes";

const SupportModule = {
  install: function (app, options) {
    registerRoute(options.router);
    registerMenu(options.menu);
  },
};
export default SupportModule;
