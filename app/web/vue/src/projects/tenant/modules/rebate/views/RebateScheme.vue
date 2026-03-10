<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div class="d-flex justify-content-between">
      <ul
        class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
      >
        <li class="nav-item">
          <a
            class="nav-link text-active-primary pb-4"
            :class="[
              { active: currentTab === RebateSchemaType.Rebate },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(RebateSchemaType.Rebate)"
            >Rebate Schema</a
          >
        </li>

        <li class="nav-item">
          <a
            class="nav-link text-active-primary pb-4"
            :class="[
              { active: currentTab === RebateSchemaType.Base },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(RebateSchemaType.Base)"
            >Base Schema</a
          >
        </li>

        <li class="nav-item">
          <a
            class="nav-link text-active-primary pb-4"
            :class="[
              { active: currentTab === RebateSchemaType.Rate },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(RebateSchemaType.Rate)"
            >Rate Schema</a
          >
        </li>

        <li class="nav-item">
          <a
            class="nav-link text-active-primary pb-4"
            :class="[
              { active: currentTab === RebateSchemaType.Pips },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(RebateSchemaType.Pips)"
            >Pips Schema</a
          >
        </li>

        <li class="nav-item">
          <a
            class="nav-link text-active-primary pb-4"
            :class="[
              { active: currentTab === RebateSchemaType.Commission },
              { 'disabled opacity-50 pe-none': isLoading },
            ]"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(RebateSchemaType.Commission)"
            >Commission Schema</a
          >
        </li>
      </ul>

      <div class="mb-5">
        <button
          class="btn btn-light btn-success btn-sm me-3"
          @click="showAddNewModal(currentTab)"
        >
          {{
            [
              "New Base Schema",
              "New Rate Schema",
              "New Pips Schema",
              "New Commission Schema",
              "New Rebate Schema",
            ][currentTab]
          }}
        </button>
      </div>
    </div>

    <div class="card mb-5 mb-xl-8">
      <div class="card-header">
        <div class="card-title">
          <el-input
            v-model="criteria.keyword"
            placeholder="Search by Name"
            @keyup.enter="fetchData(1)"
          >
            <template #append>
              <el-button :icon="Search" @click="fetchData(1)" /> </template
          ></el-input>
          <el-button class="ms-5" @click="reset">Reset</el-button>
        </div>
      </div>
      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th>{{ $t("fields.id") }}</th>
              <th>{{ $t("fields.name") }}</th>
              <th>{{ $t("fields.createdBy") }}</th>
              <th>{{ $t("fields.updatedOn") }}</th>
              <th>{{ $t("fields.createdOn") }}</th>
              <th>{{ $t("fields.note") }}</th>
              <th>{{ $t("action.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading" style="height: 1500px">
            <tr>
              <td colspan="12"><scale-loader></scale-loader></td>
            </tr>
          </tbody>
          <tbody v-if="!isLoading && rebateRules.length === 0">
            <tr>
              <td colspan="12">{{ $t("tip.nodata") }}</td>
            </tr>
          </tbody>
          <TransitionGroup
            v-if="!isLoading && rebateRules.length != 0"
            tag="tbody"
            name="table-delete-fade"
            class="table-delete-fade-container text-gray-600 fw-semibold"
          >
            <tr
              v-for="(item, index) in rebateRules"
              :key="item"
              :class="{ 'table-delete-fade-active': index === deleteIndex }"
            >
              <!--  -->
              <td>{{ item.id }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.createdByPartyName }}</td>
              <td><TimeShow :date-iso-string="item.updatedOn" /></td>
              <td><TimeShow :date-iso-string="item.createdOn" /></td>
              <td>--</td>

              <td>
                <button
                  class="btn btn-light btn-success btn-sm me-3"
                  @click="showSchemaDetail(item.id)"
                >
                  {{ $t("title.details") }}
                </button>
                <button
                  v-if="currentTab == RebateSchemaType.Rebate"
                  class="btn btn-light btn-info btn-sm me-3"
                  @click="getSchemaUsers(item.id)"
                >
                  {{ $t("title.users") }}
                </button>
                <button
                  v-if="
                    currentTab !== RebateSchemaType.Rebate &&
                    currentTab !== RebateSchemaType.Base
                  "
                  class="btn btn-light btn-danger btn-sm"
                  @click="openConfirmPanel(item.id)"
                >
                  {{ $t("action.delete") }}
                </button>
              </td>
            </tr>
          </TransitionGroup>
        </table>
        <TableFooter @page-change="pageChange" :criteria="criteria" />
      </div>
      <SchemaDetail ref="SchemaDetailRef" @refresh="refresh" />
      <SchemaUsers ref="SchemaUsersRef" />
      <AddSchama ref="AddSchamaRef" @refresh="refresh" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import TimeShow from "@/components/TimeShow.vue";
import { useI18n } from "vue-i18n";
import RebateService from "../services/RebateService";
import SchemaDetail from "../components/SchemaDetail.vue";
import SchemaUsers from "../components/SchemaUsers.vue";
import AddSchama from "../components/AddNewRebateSchema.vue";
import TableFooter from "@/components/TableFooter.vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { RebateSchemaType } from "@/core/types/RebateSchemaType";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Search } from "@element-plus/icons-vue";
const { t } = useI18n();
const isLoading = ref(true);
const deleteIndex = ref(-1);
const SchemaDetailRef = ref<any>(null);
const SchemaUsersRef = ref<any>(null);
const AddSchamaRef = ref<any>(null);
const rebateRules = ref(Array<any>());
const currentTab = ref(RebateSchemaType.Rebate);
const openConfirmBox = inject<any>(InjectKeys.OPEN_CONFIRM_MODAL);

const criteria = ref({
  page: 1,
  size: 10,
  numPages: 1,
  total: 0,
  keyword: "",
  type: RebateSchemaType.Rebate,
  sortField: "updatedOn",
  sortFlag: true,
});

const reset = () => {
  criteria.value.keyword = "";
  fetchData(1);
};

onMounted(async () => {
  fetchData(1);
});

const refresh = () => {
  fetchData(1);
};

const changeTab = async (_tab) => {
  currentTab.value = _tab;
  criteria.value.type = _tab;
  await fetchData(1);
};

const getSchemaUsers = async (_id: number) => {
  SchemaUsersRef.value.show(_id);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;

  try {
    var res;
    if (criteria.value.type == RebateSchemaType.Rebate) {
      res = await RebateService.queryRebateSchemas(criteria.value);
    } else if (criteria.value.type == RebateSchemaType.Base) {
      res = await RebateService.queryBaseRebateSchemas(criteria.value);
    } else {
      res = await RebateService.queryRebateSchemaBundle(criteria.value);
    }

    criteria.value = res.criteria;
    criteria.value.type = currentTab.value;
    rebateRules.value = res.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const showAddNewModal = (_schemaType: number) => {
  AddSchamaRef.value.show(_schemaType);
};

const pageChange = (_page: number) => {
  fetchData(_page);
};

const openConfirmPanel = (_id: number) => {
  const _handler = {
    [RebateSchemaType.Rebate]: () => RebateService.deleteRebateSchema(_id),
    [RebateSchemaType.Base]: () => RebateService.deleteBaseSchemaList(_id),
    [RebateSchemaType.Rate]: () => RebateService.deleteRebateSchemaBundle(_id),
    [RebateSchemaType.Pips]: () => RebateService.deleteRebateSchemaBundle(_id),
    [RebateSchemaType.Commission]: () =>
      RebateService.deleteRebateSchemaBundle(_id),
  }[currentTab.value];
  if (!_handler) {
    MsgPrompt.error(t("tip.fail"));
    return;
  }
  openConfirmBox(async () => {
    try {
      await _handler()
        .then(() => MsgPrompt.success())
        .then(() => {
          fetchData(criteria.value.page);
        });
    } catch (error) {
      MsgPrompt.error("有人正在使用该方案，或聯繫 IT 人員");
    }
  });
};

const showSchemaDetail = (_id: number) => {
  SchemaDetailRef.value.show(_id, currentTab.value);
};
</script>
