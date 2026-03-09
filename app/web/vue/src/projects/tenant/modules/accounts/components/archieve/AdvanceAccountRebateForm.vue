<template>
  <div class="table-responsive">
    <div class="row mb-9">
      <div class="col-3 d-flex flex-column">
        <label for="" class="required mb-3">Target Account Id</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="name"
          v-model="formData.targetAccountUid"
        />
        <ErrorMessage
          class="fv-plugins-message-container invalid-feedback"
          name="name"
          as="div"
        >
          {{ $t("tips.requiredField") }}</ErrorMessage
        >
      </div>
    </div>

    <table
      class="table table-row-dashed table-row-gray-200 align-middle gs-0 gy-4"
    >
      <thead>
        <tr class="border-0">
          <th class="p-0"></th>
          <th class="p-0 min-w-150px"></th>
          <th class="p-0 min-w-120px"></th>
          <th class="p-0 min-w-110px"></th>
        </tr>
      </thead>

      <tbody class="fw-semibold text-gray-900">
        <tr class="text-center" style="border-bottom: 1px solid #000">
          <td class="table-title-gray">Category</td>
          <td class="table-title-gray">Total Rebate</td>
          <td class="table-title-gray">Personal Rebate</td>
          <td class="table-title-gray">Remain Rebate for Sub-IB</td>
        </tr>
        <tr
          class="text-center"
          v-for="(item, index) in props.formTable"
          :key="index"
        >
          <td>{{ item.name }}</td>
          <td>--</td>
          <td>
            <el-input class="w-60px me-3" type="string" v-model="item.r" />
          </td>
          <td>--</td>
        </tr>
      </tbody>
    </table>

    <button
      class="btn btn-light btn-success btn-sm me-3 mb-9"
      @click="addDirectRule"
    >
      Add Direct Rebate Rule
    </button>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { Field, ErrorMessage, useForm } from "vee-validate";
import AccountService from "../../services/AccountService";
import Swal from "sweetalert2/dist/sweetalert2.js";

const props = defineProps<{
  accountUId: number;
  formTable: any;
}>();

const formData = ref({
  targetAccountUid: "",
});

const editableForm = ref(JSON.parse(JSON.stringify(props.formTable)));

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const addDirectRule = async () => {
  var allocateSchema = [] as any;
  editableForm.value.forEach((element) => {
    allocateSchema.push({
      cid: element.cid,
      r: parseInt(element.r),
      cr: 0,
    });
  });

  try {
    await AccountService.postDirectRebateRule({
      sourceAccountUid: props.accountUId,
      targetAccountUid: formData.value.targetAccountUid,
      schemas: allocateSchema,
    });
    Swal.fire({
      text: "You has successfully new a direct rebate rule.",
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: "Ok, got it!",
      customClass: {
        confirmButton: "btn btn-primary",
      },
    }).then(() => {
      emits("refresh");
      editableForm.value = props.formTable;
      formData.value.targetAccountUid = "";
    });
  } catch (error) {
    Swal.fire({
      text: "Sorry, looks like there are some errors detected, please try again.",
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: "Ok, got it!",
      customClass: {
        confirmButton: "btn btn-primary",
      },
    });
  }
};
</script>

<style>
.table-title-gray {
  background-color: #f5f7fa !important;
}

.table-title-blue {
  color: white !important;
  background-color: #0053ad !important;
}
</style>
