module.exports = {
  client: {
    pages: {
      index: {
        entry: `src/projects/client/client.ts`,
        filename: "index.html",
        template: "public/index.html",
        title: "Midasmarkets Pro Client",
      },
    },
    devServer: {
      host: "client.localhost",
      port: 8084,
    },
    configureWebpack: {
      devtool: "source-map",
    },
    productionSourceMap: false,
  },
  backend: {
    pages: {
      index: {
        entry: `src/projects/backend/backend.ts`,
        filename: "index.html",
        template: "public/index.html",
        title: "Midasmarkets Pro Backend",
      },
    },
    devServer: {
      host: "center.localhost",
      port: 8080,
    },
    configureWebpack: {
      devtool: "source-map",
    },
  },
  tenant: {
    pages: {
      index: {
        entry: `src/projects/tenant/tenant.ts`,
        filename: "index.html",
        template: "public/index.html",
        title: "Midasmarkets Pro Tenant",
      },
    },
    devServer: {
      host: "tenant.localhost",
      port: 8082,
    },
    configureWebpack: {
      devtool: false,
    },
    productionSourceMap: false,
  },
};
