<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-input
          v-model="criteria.email"
          class="w-300px"
          :placeholder="$t('fields.searchByEmail')"
          clearable
        />
        <el-button class="ms-2" type="primary" @click="fetchUsers(1)">
          {{ $t("action.search") }}
        </el-button>
        <el-button @click="reset()">
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>
    <div class="card-body py-4">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>Party ID</th>
            <th>{{ $t("fields.user") }}</th>
            <th>Tenant ID</th>
            <th class="text-center">{{ $t("fields.emailConfirmed") }}</th>
            <th>{{ $t("fields.phoneNum") }}</th>
            <th>{{ $t("fields.country") }}</th>
            <th>{{ $t("fields.language") }}</th>
            <th>{{ $t("fields.updatedOn") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && users.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in users" :key="index">
            <th>
              {{ item.partyId }}
              ({{ item.id }})
            </th>
            <td class="">
              <UserInfo
                v-if="item.tenantId == tenantId"
                url="#"
                :avatar="item.avatar"
                :title="item.firstName + ' ' + item.lastName"
                :sub="item.email"
                :name="item.firstName + ' ' + item.lastName"
                :user="item"
                class="me-2 d-flex align-items-center"
              />
              <template v-else>
                <div>
                  {{ item.firstName + " " + item.lastName }}
                </div>
                <div>
                  {{ item.email }}
                </div>
              </template>
            </td>
            <th>
              {{ item.tenantId }}
            </th>

            <td class="text-center">
              <i
                class="fa-solid fa-circle-check fa-lg"
                :style="item.emailConfirmed ? 'color: #5cb85c' : 'color: #gray'"
              ></i>
            </td>
            <td>+{{ item.ccc }} {{ item.phoneNumber }}</td>
            <td>{{ item.countryCode }}</td>
            <td>{{ item.language }}</td>
            <td><TimeShow :date-iso-string="item.updatedOn" /></td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>
              <button
                v-if="item.tenantId == tenantId"
                class="btn btn-light btn-success btn-sm me-3"
                @click="showDetail(item.partyId)"
              >
                {{ $t("title.details") }}
              </button>
              <el-button
                v-else
                type="primary"
                size="small"
                @click="switchTenant(item)"
                >Switch Tenant</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchUsers" :criteria="criteria" />
    </div>
  </div>
  <UserDetails ref="userShow"></UserDetails>

  <!--end::Row-->
</template>

<script lang="ts" setup>
import { onMounted, ref } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import UserDetails from "../../../components/UserDetails.vue";
import TimeShow from "@/components/TimeShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import UserService from "../services/UserService";

const tenantId = ref(parseInt(window.localStorage.getItem("tenant"), 10));
const userShow = ref<InstanceType<typeof UserDetails>>();
const users = ref(Array<any>());
const isLoading = ref(true);

const criteria = ref<any>({
  page: 1,
  size: 20,
});

onMounted(() => {
  fetchUsers(1);
});

const fetchUsers = async (_page?: number) => {
  isLoading.value = true;
  if (_page) criteria.value.page = _page;
  try {
    const results = await UserService.fetchAllUsers(criteria.value);
    users.value = results.data;
    criteria.value = results.criteria;
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
};

const switchTenant = async (item: any) => {
  isLoading.value = true;
  try {
    await UserService.switchTenant(item.partyId, item.tenantId, tenantId.value);
    fetchUsers(1);
  } catch (error) {
    console.error(error);
    fetchUsers(1);
  } finally {
    isLoading.value = false;
  }
};

const reset = () => {
  criteria.value = {
    page: 1,
    size: 20,
  };
  fetchUsers(1);
};

const showDetail = async (partyId: number) => {
  userShow.value?.show(partyId);
};
</script>
