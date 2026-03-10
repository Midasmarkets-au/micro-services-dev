export default (menu) => {
  menu.addMenu([
    {
      heading: "title.contacts",
      route: "/contacts",
      pages: [
        {
          heading: "title.Contact",
          route: "/contacts/contacts",
          svgIcon: "/images/icons/communication/com005.svg",
          permissions: ["contact.view"],
        },
        {
          heading: "title.department",
          route: "/contacts/departments",
          svgIcon: "/images/icons/ecommerce/ecm008.svg",
          permissions: ["department.view"],
        },
        {
          heading: "title.Office",
          route: "/contacts/offices",
          svgIcon: "/images/icons/general/gen018.svg",
          permissions: ["office.view"],
        },
      ],
    },
  ]);
};
