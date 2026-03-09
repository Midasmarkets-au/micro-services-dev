import { computed } from "vue";

export const AccessRoles = computed(() => [
  { label: "Client", value: "Client" },
  { label: "Guest", value: "Guest" },
  { label: "IB", value: "IB" },
  { label: "Sales", value: "Sales" },
  { label: "EventShopTest", value: "EventShopTest" },
]);

export const PointRoles = computed(() => [
  { label: "All", value: "all" },
  { label: "Agents", value: "agent" },
  { label: "Client", value: "client" },
  { label: "Sales", value: "sales" },
]);
