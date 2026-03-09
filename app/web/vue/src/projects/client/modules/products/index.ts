import registerRoute from "./routes";
import registerMenu from "./menu";
const ProductsModule = {
  install: function (app, options) {
    registerRoute(options.router);
    registerMenu(options.menu);
  },
};
export default ProductsModule;
