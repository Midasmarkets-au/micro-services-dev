<template>
  <SimpleForm
    ref="bankInfoDetailShowRef"
    :title="'Bank Information'"
    :is-loading="isLoading"
    :width="500"
    disable-footer
  >
    <div>
      <div v-if="isLoading" class="d-flex justify-content-center">
        <LoadingRing />
      </div>
      <div v-else>
        <div
          class="key-value-pair"
          v-for="(value, key) in bankInfo.info"
          :key="key"
        >
          <label class="required fs-6 fw-semobold mb-1 createAccountTitle">
            <!-- {{ $t("fields." + key) }} -->
            {{ key }}
          </label>
          <Field
            v-model="bankInfo.info[key]"
            class="form-control form-control-lg form-control-solid"
            type="number"
            name="symbolRule"
            autocomplete="off"
          >
            <el-input class="mb-3" v-model="bankInfo.info[key]" size="large">
            </el-input>
          </Field>
        </div>

        <div class="mt-7 d-flex flex-row-reverse">
          <button class="btn btn-light btn-danger btn-lg me-3" @click="hide">
            Cancel
          </button>
          <button class="btn btn-light btn-success btn-lg me-3" @click="update">
            Update
          </button>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, nextTick } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Field } from "vee-validate";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const isLoading = ref(true);
const bankInfoDetailShowRef = ref<any>(null);
const bankInfo = ref({} as any);

const update = async () => {
  try {
    await PaymentService.putPaymentInformation(
      bankInfo.value.id,
      bankInfo.value
    );
    emits("refresh");
    bankInfoDetailShowRef.value?.hide();
  } catch (e) {
    MsgPrompt.error(e);
  }
};

const show = async (_item: any) => {
  bankInfo.value = _item;
  isLoading.value = false;
  bankInfoDetailShowRef.value?.show();
};

const hide = async () => {
  bankInfoDetailShowRef.value?.hide();
};

defineExpose({ show });
</script>

<style>
.rebate-long-btn {
  width: 100%;
  height: 40px;
  font-size: 24px;
  border-radius: 8px;
  color: gray;
  background-color: white;
  border: 1px solid lightgray;
}

.rebate-long-btn:focus {
  background-color: lightgray;
}
</style>
