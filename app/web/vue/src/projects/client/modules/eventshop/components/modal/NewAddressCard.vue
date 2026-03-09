<template>
  <div v-if="data.length === 0">
    <el-button @click="addAddress()">{{ $t("title.addAddress") }}</el-button>
  </div>
  <el-button
    v-else
    plain
    @click="showAddressList()"
    class="d-flex w-100 py-3 justify-content-between position-relative"
    style="height: auto"
  >
    <div class="d-flex flex-column gap-3">
      <div
        class="d-flex flex-sm-row flex-column align-items-sm-center align-items-start gap-2"
      >
        <span class="text-black">{{ form.value?.name }}</span>
        <span>{{ form.value?.ccc }} {{ form.value?.phone }}</span>
      </div>
      <div class="d-flex flex-row align-items-center gap-1">
        <span>{{ form.value?.content?.address }},</span>
        <span>{{ form.value?.content?.city }},</span>
        <span>{{ form.value?.content?.state }},</span>
        <span>{{ form.value?.content?.postalCode }}</span>
      </div>
    </div>
    <div class="position-absolute end-5 top-sm-1/2 top-5 mt-sm-2">
      <i class="bi bi-pencil-fill fs-7"></i>
    </div>
  </el-button>
  <AddAddress ref="addAddressRef" @submit="fetchData" />
  <AddressList ref="AddressListRef" @choose-address="updateAddress" />
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from "vue";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import AddAddress from "@/projects/client/views/profile/components/AddAddress.vue";
import AddressList from "./AddressList.vue";
const dialogFormVisible = ref(false);
const isLoading = ref(false);
const data = ref(<any>[]);
const addAddressRef = ref<InstanceType<typeof AddAddress>>();
const AddressListRef = ref<InstanceType<typeof AddressList>>();
const form = reactive({});
const addressHashId = ref("hi");
const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await ClientGlobalService.queryAddressList();
    data.value = res.data;
    form.value = data.value[0];
    addressHashId.value = form.value?.hashId;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const addAddress = () => {
  addAddressRef.value.show();
};
const showAddressList = () => {
  AddressListRef.value.show(form.value);
};
const updateAddress = (item: any) => {
  form.value = item;
  addressHashId.value = item.hashId;
};
onMounted(() => {
  fetchData();
});
defineExpose({
  addressHashId,
});
</script>

<style lang="scss">
.address-modal {
  .el-dialog__body {
    padding-top: 0;
    padding-bottom: 0;
  }
  .el-form-item {
    margin-bottom: 0;
  }
}
.label-title {
  font-weight: 400;
  line-height: 20px;
  color: #000000;
}
</style>
