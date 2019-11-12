const ReportManager = {
    getStudentsInCohort(cohortId) {
        return fetch(`/api/ReportData/GetStudentsByCohortId/${cohortId}`)
                .then(resp => resp.json());
    }
}