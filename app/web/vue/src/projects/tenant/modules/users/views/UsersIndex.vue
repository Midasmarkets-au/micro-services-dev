<template>
  <div class="card">
    <div class="card-header">
      <!-- <div class="card-title">{{ $t("title.users") }}</div>
      <div class="card-toolbar">
        <el-input
          v-model="criteria.partyId"
          class="w-100 m-2"
          :placeholder="$t('tip.pleaseInput')"
          :suffix-icon="Search"
          @keyup.enter="fetchUsers(1)"
        />
      </div> -->
      <div class="d-flex align-items-center gap-4">
        <h2 class="m-0">{{ $t("title.user") }}</h2>
        <!-- <SearchFilter
          search-types="user"
          @get-results-ids="handleSearchResults"
          :defaultCriteria="criteria"
          :search-trigger="searchTrigger"
          multiple-selection
          require-empty-search
        /> -->
        <el-input
          v-model="criteria.searchText"
          style="max-width: 600px"
          :placeholder="$t('tip.searchKeyWords')"
          clearable
          class="w-300px"
          :disabled="isLoading"
          v-on:keyup.enter="fetchData(1)"
        >
          <template #append>
            <el-button
              :icon="Search"
              @click="fetchData(1)"
              :disabled="isLoading"
            />
          </template>
        </el-input>
        <el-button :disabled="isLoading" @click="reset()">{{
          $t("action.reset")
        }}</el-button>
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
            <th>{{ $t("fields.site") }}</th>
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
            <td class="d-flex align-items-center">
              <UserInfo
                url="#"
                :avatar="item.avatar"
                :title="getUserName(item)"
                :sub="item.email"
                :name="getUserName(item)"
                :user="item"
                class="me-2"
              />
            </td>
            <th>
              {{ $t("type.siteType." + item.siteId) }}
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
                class="btn btn-light btn-success btn-sm me-3"
                @click="showDetail(item.partyId)"
              >
                {{ $t("title.details") }}
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
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
// import SearchFilter from "@/components/SearchFilter.vue";
import UserService from "../services/UserService";
import { Search } from "@element-plus/icons-vue";

const userShow = ref<InstanceType<typeof UserDetails>>();
const users = ref(Array<any>());
const isLoading = ref(true);
// const searchTrigger = ref(false);

const criteria = ref<any>({
  page: 1,
  size: 20,
  total: 0,
});

onMounted(() => {
  fetchData(1);
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const results = await UserService.getUsers(criteria.value);
    criteria.value = results.criteria;
    users.value = results.data;
  } catch (e) {
    console.error(e);
  }
  isLoading.value = false;
};

// const fetchUsers = async (_page?: number, shouldTrigger = true) => {
//   isLoading.value = true;
//   users.value = [];
//   if (_page) criteria.value.page = _page;
//   shouldTrigger && (searchTrigger.value = !searchTrigger.value);
//   await nextTick();
//   isLoading.value = false;
// };

// const handleSearchResults = async (results) => {
//   isLoading.value = true;
//   users.value = results.data;
//   criteria.value = results.criteria;
//   isLoading.value = false;
// };

const showDetail = async (partyId: number) => {
  userShow.value?.show(partyId);
};

const reset = async () => {
  criteria.value = {
    page: 1,
    size: 20,
    total: 0,
  };
  await fetchData(1);
};

const getUserName = (item: any) => {
  if (!item.nativeName || item.nativeName === "" || item.nativeName === " ") {
    if (
      !item.displayName ||
      item.displayName === "" ||
      item.displayName === " "
    ) {
      if (
        !item.firstName ||
        !item.lastName ||
        item.firstName === "" ||
        item.lastName === "" ||
        item.firstName === " " ||
        item.lastName === " "
      ) {
        return "No Name";
      } else {
        return item.firstName + " " + item.lastName;
      }
    } else {
      return item.displayName;
    }
  } else {
    return item.nativeName;
  }
};
</script>
