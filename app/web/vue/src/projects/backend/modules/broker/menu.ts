export default (menu) => {
  menu.addMenu([
    {
      heading: "title.broker",
      route: "/broker",
      pages: [
        {
          heading: "title.broker",
          route: "/broker",
          svgIcon: "/images/icons/general/gen022.svg",
          permissions: ["broker.view"],
        },
      ],
    },
  ]);
};
