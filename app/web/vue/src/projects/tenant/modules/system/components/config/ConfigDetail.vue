<template>
  <SimpleForm
    ref="modalRef"
    :title="title"
    :is-loading="isLoading"
    :submit="submit"
    :before-close="hide"
    :width="900"
  >
    <div class="my-4">
      <el-form-item label="Value" label-position="right" label-width="100px"
        ><el-input
          label="value"
          v-model="value"
          :autosize="{ minRows: 1, maxRows: 6 }"
          type="textarea"
        ></el-input
      ></el-form-item>
      <el-form-item
        label="Description"
        label-position="right"
        label-width="100px"
      >
        <el-input v-model="data.description"></el-input
      ></el-form-item>
    </div>
  </SimpleForm>
</template>
<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import SystemService from "../../services/SystemService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
const isLoading = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const category = ref("");
const rowId = ref(0);
const key = ref("");
const data = ref(<any>[]);
const value = ref<any>("");
const title = ref("");

const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.queryConfigByKey(
      category.value,
      rowId.value,
      key.value
    );
    data.value = res;

    if (data.value) {
      data.value["valueString"] = JSON.stringify(res.value);
      title.value = data.value.name;
      value.value = data.value.valueString;
    }
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const show = async (
  _category: string,
  _rowId: number,
  _key: string,
  _data?: string
) => {
  // category.value = capitalizeFirstLetter(_category);
  category.value = _category;
  rowId.value = _rowId;
  key.value = _key;

  await fecthData();

  if (data.value === null || data.value === undefined || data.value === "") {
    data.value = _data;
    title.value = data.value.name;
    value.value = data.value.valueString;
  }
  modalRef.value?.show();
};

const submit = async () => {
  try {
    await SystemService.updateConfigByKey(
      category.value,
      rowId.value,
      key.value,
      {
        value: JSON.parse(value.value),
        key: data.value.key,
        name: data.value.name,
        description: data.value.description,
        dataFormat: data.value.dataFormat,
      }
    )
      .then(() => {
        MsgPrompt.success("Update successfully");
      })
      .then(() => {
        emit("submitted");
        hide();
      });
  } catch (e) {
    console.log(e);
  }
};

function capitalizeFirstLetter(string) {
  return string.charAt(0).toUpperCase() + string.slice(1);
}
const emit = defineEmits(["submitted"]);
defineExpose({
  show,
  hide,
});
</script>
