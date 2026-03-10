import registerRoute from "./routes";
import registerMenu from "./menu";

const ShopModule = {
  install: function (app, options) {
    registerRoute(options.router);
    registerMenu(options.menu);
  },
};
export default ShopModule;
