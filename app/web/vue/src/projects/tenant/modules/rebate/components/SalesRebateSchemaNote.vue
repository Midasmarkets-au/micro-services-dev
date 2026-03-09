<template>
  <SimpleForm
    ref="salesRebateSchemaNoteShowRef"
    :title="'Schema Note'"
    :is-loading="false"
    :width="500"
    disable-footer
  >
    <div>
      <div class="row mb-5">
        <div class="col-12">
          <label class="required fs-6 fw-semobold mb-2 createAccountTitle">
            Note
          </label>
          <Field
            v-model="schemaInfo.note"
            class="form-control form-control-lg form-control-solid"
            name="symbolRule"
            autocomplete="off"
          >
            <el-input
              v-model="schemaInfo.note"
              size="large"
              :rows="5"
              type="textarea"
            >
            </el-input>
          </Field>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, nextTick } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import { Field } from "vee-validate";

const salesRebateSchemaNoteShowRef = ref<any>(null);
const schemaInfo = ref({} as any);

const show = async (_item: any) => {
  salesRebateSchemaNoteShowRef.value?.show();
  schemaInfo.value = _item;
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
