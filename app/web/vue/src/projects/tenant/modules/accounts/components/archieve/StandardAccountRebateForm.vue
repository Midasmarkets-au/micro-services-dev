<template>
  <div class="table-responsive">
    <div class="row mb-9">
      <div class="col-3 d-flex flex-column">
        <label for="" class="required mb-3">Target Account Number</label>
        <Field
          type="text"
          class="form-control form-control-solid"
          name="name"
          v-model="formData.targetAccountUid"
          @keyup="getAccountInfo($event)"
        />
        <ErrorMessage
          class="fv-plugins-message-container invalid-feedback"
          name="name"
          as="div"
        >
          {{ $t("tips.requiredField") }}</ErrorMessage
        >
        <div v-if="!isAccountExist" style="color: #900000">
          Account not exist
        </div>
      </div>
    </div>

    <table
      class="table table-row-dashed table-row-gray-200 align-middle gs-0 gy-4"
    >
      <tbody class="fw-semibold text-gray-900">
        <tr class="text-center" style="border-bottom: 1px solid #000">
          <td class="table-title-gray">Category</td>
          <td class="table-title-gray">Total Rebate</td>
          <td class="table-title-gray">Personal Rebate</td>
          <td class="table-title-gray">Remain Rebate for Sub-IB</td>

          <td class="table-title-gray">Percentage</td>
        </tr>
        <tr
          class="text-center"
          v-for="(item, index) in editableForm"
          :key="index"
        >
          <td>{{ item.name }}</td>
          <td>--</td>
          <td>
            <el-input class="w-60px me-3" type="string" v-model="item.r" />
          </td>
          <td>--</td>

          <td
            v-if="index == 0"
            :rowspan="editableForm.length"
            style="border-left: 1px solid #f5f5f5"
          >
            <div
              class="d-flex align-items-center justify-content-center p-2"
              style="cursor: pointer"
            >
              <span
                ><el-input
                  class="w-60px me-3"
                  type="string"
                  v-model="formData.percentage"
                />
                %</span
              >
            </div>
          </td>
        </tr>
      </tbody>
    </table>

    <div class="mt-13 d-flex flex-row-reverse">
      <button
        class="btn btn-light btn-success btn-lg me-3 mb-9"
        @click="addDirectRule"
      >
        Add Direct Rebate Rule
      </button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import AccountService from "../../services/AccountService";
import { Field, ErrorMessage, useForm } from "vee-validate";
import Swal from "sweetalert2/dist/sweetalert2.js";

const props = defineProps<{
  accountUId: number;
  formTable: any;
}>();

const formData = ref({
  percentage: "0",
  targetAccountUid: "",
});

const isAccountExist = ref(true);
const sourceAccountUid = ref("");
const editableForm = ref(JSON.parse(JSON.stringify(props.formTable)));

const emits = defineEmits<{
  (e: "refresh"): void;
}>();

const getAccountInfo = async (event: any) => {
  if (event.target.value.length >= 7) {
    try {
      const res = await AccountService.getTradeAccount({
        AccountNumber: event.target.value,
      });
      console.log(res);
      // sourceAccountUid.value = res.
      isAccountExist.value = true;
    } catch (error) {
      isAccountExist.value = false;
    }
  }
};

const addDirectRule = async () => {
  var allocateSchema = [] as any;
  editableForm.value.forEach((element) => {
    allocateSchema.push({
      cid: element.cid,
      r: parseInt(element.r),
      cr: parseInt(formData.value.percentage),
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
      formData.value.percentage = "0";
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

.rebateDropdown {
  position: absolute;
  overflow: hidden;
  right: 10px;

  color: black;
  background-color: white;
  width: 172px;
  filter: drop-shadow(0px 4px 14px rgba(0, 0, 0, 0.1));
  border-radius: 8px;
}

.dropdownItem {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 40px;
  cursor: pointer;
  border: 1px solid #f5f7fa;
}

.dropdownItem:hover {
  background-color: #f5f7fa;
}
</style>
