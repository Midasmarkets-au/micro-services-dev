<template>
  <SimpleForm ref="modalRef" title="Customer Detail" :is-loading="isSubmitting">
    <div class="px-5">
      <div>
        <div class="d-flex gap-10">
          <div class="borders">
            <div class="head">Name</div>
            <div class="content">{{ data.customer.name }}</div>
          </div>
          <div class="borders">
            <div class="head">Phone</div>
            <div class="content">{{ data.customer.phone }}</div>
          </div>
        </div>
        <div class="borders w-100 text-start mt-5">
          <div class="head">Address</div>
          <div class="content">{{ data.customer.address }}</div>
        </div>
        <div class="d-flex gap-10 mt-5">
          <div class="borders">
            <div class="head">Zipcode</div>
            <div class="content">{{ data.customer.zipcode }}</div>
          </div>
          <div class="borders">
            <div class="head">Country</div>
            <div class="content">{{ data.customer.country }}</div>
          </div>
        </div>
        <div class="borders w-100 text-start mt-5">
          <div class="head">Comment</div>
          <div class="content">{{ data.customer.comment }}</div>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const emits = defineEmits<{
  (e: "submit"): void;
}>();
const isSubmitting = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();

const hide = () => {
  modalRef.value?.hide();
};

const data = ref<any>({});

const show = async (_item: any) => {
  if (_item != null) {
    data.value = _item;
  }
  modalRef.value?.show();
};

defineExpose({
  show,
  hide,
});
</script>

<style scoped>
.borders {
  /* collapse border */
  text-align: center;
  width: 150px;
  border: 1px solid #ccc;
  padding: 10px;
  margin-left: 2px;
  margin-right: 2px;
  border-radius: 5px;
}
.head {
  font-weight: bold;
  font-size: 14px;
  padding-bottom: 2px;
}
.title {
  font-weight: bold;
  font-size: 16px;
}
</style>
