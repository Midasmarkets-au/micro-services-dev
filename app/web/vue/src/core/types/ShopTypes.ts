export enum CustomerStatus {
  Pending = 0,
  Rejected = 1,
  Approved = 2,
}

export const customerStatusOptions = [
  { label: "Pending", value: CustomerStatus.Pending },
  { label: "Rejected", value: CustomerStatus.Rejected },
  { label: "Approved", value: CustomerStatus.Approved },
  { label: "All", value: "" },
];

export enum OrderStatus {
  Pending = 0,
  Rejected = 1,
  Approved = 2,
  Shippded = 3,
  Delivered = 4,
}

export const orderStatusOptions = [
  { label: "Pending", value: OrderStatus.Pending },
  { label: "Rejected", value: OrderStatus.Rejected },
  { label: "Approved", value: OrderStatus.Approved },
  { label: "Shippded", value: OrderStatus.Shippded },
  { label: "Delivered", value: OrderStatus.Delivered },
  { label: "All", value: "" },
];
