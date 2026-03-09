const initGlobalFilter = (app) => {
  app.config.globalProperties.$filters = {
    digits(value, digits = 2) {
      return Math.round(value * Math.pow(10, digits)) / Math.pow(10, digits);
    },
  };
};

declare module "@vue/runtime-core" {
  interface ComponentCustomProperties {
    $filters: {
      digits(value: number, digits?: number): number;
    };
  }
}

export default initGlobalFilter;
