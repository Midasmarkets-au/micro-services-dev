<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.assignSales')"
    :is-loading="isLoading"
    :submit="submit"
    :width="300"
  >
    <div class="row w-100 overflow-hidden">
      <!--      <div class="col-5 d-flex flex-column justify-content-around">-->
      <!--        <div-->
      <!--          v-for="item in selectedLeads"-->
      <!--          :key="item.id"-->
      <!--          class="pb-1 mb-1 border-bottom border-bottom-1"-->
      <!--        >-->
      <!--          <div class="d-flex align-items-center gap-3">-->
      <!--            <i class="fa-regular fa-user"></i>-->
      <!--            <div class="fw-bold fs-5">{{ item.name }}</div>-->
      <!--          </div>-->
      <!--          <div class="d-flex align-items-center gap-3">-->
      <!--            <i class="fa-regular fa-envelope"></i>-->
      <!--            <div>{{ item.email }}</div>-->
      <!--          </div>-->
      <!--        </div>-->
      <!--      </div>-->

      <!--      <div class="col-2 d-flex justify-content-center align-items-center">-->
      <!--        <div class="d-flex flex-column gap-5">-->
      <!--          <span><i class="fa-solid fa-arrow-right-long fa-2xl"></i></span>-->
      <!--          <span><i class="fa-solid fa-arrow-right-long fa-2xl"></i></span>-->
      <!--          <span><i class="fa-solid fa-arrow-right-long fa-2xl"></i></span>-->
      <!--        </div>-->
      <!--      </div>-->

      <div class="d-flex flex-column justify-content-center">
        <div>
          <label class="required fs-6 fw-semobold mb-2 mt-4">
            {{ $t("fields.salesCode") }}
          </label>
          <el-form-item>
            <el-input
              class="w-100"
              clearable
              v-model="salesGroupName"
              :disabled="isLoading"
              @blur="fetchData"
              @keydown.enter="fetchData"
            >
              <template #append>
                <el-button :icon="Search" @click="fetchData" />
              </template>
            </el-input>
          </el-form-item>
        </div>

        <div>
          <label class="required fs-6 fw-semobold mb-2 mt-4">
            {{ $t("fields.salesInfo") }}
          </label>
          <el-form-item>
            <el-input disabled v-model="assignedSalesAccount.name" />
          </el-form-item>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import SimpleForm from "@/components/SimpleForm.vue";
import { ref } from "vue";
import { Search } from "@element-plus/icons-vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "../../services/UserService";
const modalRef = ref<any>();
const isLoading = ref(true);
const selectedLead = ref({} as any);
const salesGroupName = ref(null);
const assignedSalesAccount = ref({} as any);

const emits = defineEmits<{
  (e: "assign-success"): void;
}>();

const clearNewSalesAccount = () => {
  assignedSalesAccount.value = {} as any;
  salesGroupName.value = null;
};

const fetchData = async () => {
  if (salesGroupName.value === null || salesGroupName.value === "") {
    clearNewSalesAccount();
    return;
  }
  isLoading.value = true;
  try {
    const res = await AccountService.queryAccounts({
      role: AccountRoleTypes.Sales,
      code: salesGroupName.value,
    });
    if (res.data.length === 0) {
      MsgPrompt.error("Sales account not found");
    } else {
      assignedSalesAccount.value = res.data[0];
    }
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

const submit = async () => {
  if (assignedSalesAccount.value.id === undefined) {
    MsgPrompt.error("Please specify a sales account");
    return;
  }
  try {
    isLoading.value = true;
    await UserService.assignLeadToSalesAccount(
      selectedLead.value.id,
      assignedSalesAccount.value.uid
    );
    MsgPrompt.success("Assign success!").then(() => {
      emits("assign-success");
      modalRef.value.hide();
    });
  } catch (err) {
    MsgPrompt.error(err);
    return;
  } finally {
    isLoading.value = false;
  }
};

const show = (_lead) => {
  isLoading.value = true;
  modalRef.value.show();
  selectedLead.value = _lead;
  isLoading.value = false;
};

defineExpose({
  show,
  hide: () => {
    modalRef.value.hide();
  },
});
</script>

<style scoped></style>
