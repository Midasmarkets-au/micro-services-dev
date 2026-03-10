let projectName = process.env.PROJECT_NAME;
console.log(projectName);

const config = require("./config.js");

module.exports = {
  ...config[projectName],
  lintOnSave: process.env.DOCKER_BUILD ? false : "default",
  publicPath: process.env.VUE_APP_PATH,
  devServer: {
    ...config[projectName]?.devServer,
    proxy: {
      "/images": {
        target: "https://mm-front-public.s3.ap-southeast-2.amazonaws.com",
        changeOrigin: true,
        secure: true,
      },
      "/apk": {
        target: "https://mm-front-public.s3.ap-southeast-2.amazonaws.com",
        changeOrigin: true,
        secure: true,
      },
      "/audios": {
        target: "https://mm-front-public.s3.ap-southeast-2.amazonaws.com",
        changeOrigin: true,
        secure: true,
      },
      "/fonts": {
        target: "https://mm-front-public.s3.ap-southeast-2.amazonaws.com",
        changeOrigin: true,
        secure: true,
      },
      "/fonticon": {
        target: "https://mm-front-public.s3.ap-southeast-2.amazonaws.com",
        changeOrigin: true,
        secure: true,
      },
    },
  },
  configureWebpack: {
    devtool: process.env.DISABLE_FORK_TS_CHECKER ? false : "source-map",
  },
  productionSourceMap: false,
  outputDir: process.env.PROJECT_NAME
    ? `dist/${process.env.PROJECT_NAME}`
    : "dist",
  parallel: false,
  css: {
    loaderOptions: {
      css: {
        url: {
          filter: (url) => !url.startsWith("/images/") && !url.startsWith("/apk/") && !url.startsWith("/audios/") && !url.startsWith("/fonts/") && !url.startsWith("/fonticon/"),
        },
      },
    },
  },
  chainWebpack: (config) => {
    if (process.env.DISABLE_FORK_TS_CHECKER) {
      config.plugins.delete("fork-ts-checker");
    }
  },
};
