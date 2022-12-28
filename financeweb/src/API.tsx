import axios from 'axios'

//interface for the Helper
interface Params {
    baseUrl: string
    headers : any
    method: string
}

//helper config
const postConfig: Params = {
    baseUrl: "https://jsonplaceholder.typicode.com/",
    headers: {
        "Authorization": "",
            },
    method: 'post'
}

//helper function to be exported
export const postAPI = async (url: string, data: any): Promise<any> =>{
    return await axios({
        ...postConfig,
        url: `${postConfig.baseUrl}/${url}`,
        data
    }).then ( (response) => {
        console.log(response)
        return {
            status: response.status,
            data: response.data
        }
    }).catch((error) =>{
        console.log(error)
        return {
            status: error.status,
            data: error.response
        }
    })
}

//config for get request note that the method as changed to get this is very important
const getConfig : Params = {
    baseUrl: "https://localhost:7171",
        headers: {},
    method: 'get'
}


export const getAPI = async (url: string): Promise<any> =>{
    return await axios({
        ...getConfig,
        url: `${getConfig.baseUrl}/${url}`,
    }).then ( (response) => {
        console.log(response)
        return {
            status: response.status,
            data: response.data
        }
    }).catch((error) =>{
        console.log(error)
        return {
            status: error.status,
            data: error.response
        }
    })
}