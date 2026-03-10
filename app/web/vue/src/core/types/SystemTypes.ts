import { axiosInstance as axios } from "@/core/services/api.client";

// const SystemTypes = (await axios.get("api/type")).data;

let cached = null;

const getSystemTypes = async () => {
  if (cached) {
    return cached;
  }
  const SystemTypes = (await axios.get("api/type")).data;
  cached = SystemTypes;
  return SystemTypes;
};

export default getSystemTypes;
