import registerRoute from "./routes";
// import registerMenu from "./menu";
import RepStoreModule from "./store/RepModule";

const RepModule = {
  install: function (app, options) {
    registerRoute(options.router);
    // registerMenu(options.menu);
    options.store.registerModule("RepModule", RepStoreModule);
  },
};

export default RepModule;
