<template>
  <SimpleForm
    ref="modalRef"
    title="Are you confirm to cancel this order?"
    :is-loading="isSubmitting"
    :submit="submit"
    :before-close="hide"
  >
    <div class="px-5">
      <div>
        <el-form
          ref="ruleFormRef"
          :model="formData"
          :rules="rules"
          label-position="top"
        >
          <el-form-item label="Reason for Cancel" prop="comment">
            <el-input
              v-model="formData.customer.comment"
              :autosize="{ minRows: 6, maxRows: 10 }"
              type="textarea"
            />
          </el-form-item>
        </el-form>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import type { FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const emits = defineEmits<{
  (e: "submit"): void;
}>();
const isSubmitting = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();

const hide = () => {
  modalRef.value?.hide();
};

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  comment: [
    {
      required: true,
      message: "Please input tracking number",
      trigger: "blur",
    },
  ],
});
const formData = ref<any>({});

const show = async (_item: any) => {
  if (_item != null) {
    formData.value = _item;
  }
  modalRef.value?.show();
};

const submit = async () => {
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  try {
    // const res = await ShopService.updateItem(formData.value);
    // MsgPrompt.success("Item updated successfully");
  } catch (error) {
    console.log(error);
  }
  emits("submit");
};
defineExpose({
  show,
  hide,
});
</script>

<style lang="scss">
.image {
  width: 200px;
  border-radius: 10px;
}
</style>
