const bankRequirement = {
  2: {},
  10: {},
  210: {},
  100: {
    accountNumber: {
      key: "accountNumber",
      name: "accountNumber",
      type: String,
      required: true,
      value: "",
    },
    accountName: {
      key: "accountName",
      name: "accountName",
      type: String,
      required: true,
      value: "",
    },
    routingNumber: {
      key: "routingNumber",
      name: "routingNumber",
      type: String,
      required: true,
      value: "",
    },
    date: {
      key: "Date",
      name: "Date",
      type: Date,
      required: true,
      value: "",
    },
    reference: {
      key: "reference",
      name: "reference",
      type: String,
      required: true,
      value: "",
    },
  },
  20: {
    lastName: {
      key: "lastName",
      name: "lastName",
      type: String,
      required: true,
      value: "",
    },
    fistName: {
      key: "firstName",
      name: "firstName",
      type: String,
      required: true,
      value: "",
    },
    accountNumber: {
      key: "accountNumber",
      name: "accountNumber",
      type: String,
      required: true,
      value: "",
    },
    confirmAccountnumber: {
      key: "confirmAccountnumber",
      name: "confirmAccountnumber",
      type: String,
      required: true,
      value: "",
    },
  },
};

export default bankRequirement;
