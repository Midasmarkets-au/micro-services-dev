// import registerRoute from "./routes";
import registerMenu from "./menu";
const ReportsModule = {
  install: function (app, options) {
    // registerRoute(options.router);
    registerMenu(options.menu);
  },
};
export default ReportsModule;
