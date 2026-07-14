<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title gap-3 flex-wrap">
        <el-input
          v-model="criteria.keyword"
          placeholder="Keyword"
          clearable
          style="width: 260px"
          :disabled="isLoading"
          @keyup.enter="fetchData(1)"
          @clear="fetchData(1)"
        >
          <template #append>
            <el-button
              :icon="Search"
              :loading="isLoading"
              @click="fetchData(1)"
            />
          </template>
        </el-input>

        <el-select
          v-model="criteria.isArchived"
          style="width: 180px"
          :disabled="isLoading"
          @change="fetchData(1)"
        >
          <el-option :value="false" :label="$t('status.active')" />
          <el-option :value="true" :label="$t('status.archived')" />
        </el-select>

        <el-button :disabled="isLoading" @click="reset">
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>

    <div class="card-body overflow-auto">
      <table class="table align-middle table-row-dashed fs-6 gy-5 table-hover">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("fields.id") }}</th>
            <th>{{ $t("fields.name") }}</th>
            <th>{{ $t("fields.email") }}</th>
            <th>{{ $t("fields.phone") }}</th>
            <th>{{ $t("fields.ip") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && contacts.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold text-gray-700">
          <tr v-for="item in contacts" :key="item.id">
            <td>{{ item.id }}</td>
            <td>{{ item.name }}</td>
            <td>{{ item.email }}</td>
            <td>{{ item.phoneNumber }}</td>
            <td>{{ item.ip }}</td>
            <td>
              <TimeShow type="inFields" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <el-tag :type="item.isArchived === 1 ? 'info' : 'success'">
                {{
                  item.isArchived === 1
                    ? $t("status.archived")
                    : $t("status.active")
                }}
              </el-tag>
            </td>
            <td>
              <div class="d-flex gap-2 flex-wrap">
                <el-button
                  size="small"
                  type="success"
                  @click="showDetail(item)"
                >
                  {{ $t("action.showDetails") }}
                </el-button>

                <el-button
                  v-if="item.isArchived !== 1"
                  size="small"
                  type="primary"
                  @click="showEmail(item)"
                >
                  {{ $t("action.sendEmail") }}
                </el-button>

                <el-popconfirm
                  v-if="item.isArchived !== 1"
                  :title="`${$t('action.confirm')} ${$t('action.archive')}?`"
                  :confirm-button-text="$t('action.confirm')"
                  @confirm="archive(item.id)"
                >
                  <template #reference>
                    <el-button size="small" type="warning">
                      {{ $t("action.archive") }}
                    </el-button>
                  </template>
                </el-popconfirm>

                <el-popconfirm
                  v-else
                  :title="`${$t('action.confirm')} ${$t('action.restore')}?`"
                  :confirm-button-text="$t('action.confirm')"
                  @confirm="unarchive(item.id)"
                >
                  <template #reference>
                    <el-button size="small" type="warning">
                      {{ $t("action.restore") }}
                    </el-button>
                  </template>
                </el-popconfirm>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>

    <ContactDetailModal ref="detailModalRef" />
    <ContactEmailModal ref="emailModalRef" @sent="fetchData(1)" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { Search } from "@element-plus/icons-vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import ContactService from "../services/ContactService";
import ContactDetailModal from "../components/ContactDetailModal.vue";
import ContactEmailModal from "../components/ContactEmailModal.vue";

const contacts = ref<Array<any>>([]);
const isLoading = ref(false);
const detailModalRef = ref<InstanceType<typeof ContactDetailModal>>();
const emailModalRef = ref<InstanceType<typeof ContactEmailModal>>();
const criteria = ref<any>({
  page: 1,
  size: 20,
  isArchived: false,
  sortField: "createdOn",
});

const fetchData = async (_page = 1) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await ContactService.queryContacts(criteria.value);
    contacts.value = res.data ?? res;
    criteria.value = {
      ...criteria.value,
      ...(res.criteria ?? {}),
      isArchived: criteria.value.isArchived,
    };
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

const reset = async () => {
  criteria.value = {
    page: 1,
    size: 20,
    isArchived: false,
    sortField: "createdOn",
  };
  await fetchData(1);
};

const showDetail = (item: any) => {
  detailModalRef.value?.show(item);
};

const showEmail = (item: any) => {
  emailModalRef.value?.show(item);
};

const archive = async (id: number) => {
  try {
    await ContactService.archiveContact(id);
    MsgPrompt.success();
    await fetchData(criteria.value.page);
  } catch (e) {
    MsgPrompt.error(e);
  }
};

const unarchive = async (id: number) => {
  try {
    await ContactService.unarchiveContact(id);
    MsgPrompt.success();
    await fetchData(criteria.value.page);
  } catch (e) {
    MsgPrompt.error(e);
  }
};

onMounted(async () => {
  await fetchData(1);
});
</script>
